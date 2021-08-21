using Godot;
using Ecs;
using System.Collections.Generic;
using System;

public class Combat : Manager
{
    private ProfileCardPrefab leftProfileCard;
    private ProfileCardPrefab rightProfileCard;
    private ActionMenuPrefab actionMenu;

    public override void _Ready()
    {
        leftProfileCard = GetNode("HUD/LeftProfile") as ProfileCardPrefab;
        rightProfileCard = GetNode("HUD/RightProfile") as ProfileCardPrefab;
        actionMenu = GetNode("HUD/LeftProfile/ActionMenu") as ActionMenuPrefab;
        actionMenu.SetButtonCallback(skill => AddComponentToEntity(GetNewEntity(), new SelectActionEvent() { SelectedSkill = skill }));

        ApplyState(new CombatStartState());
        CreateSystems();
        var mapComp = BuildMap();
        BuildControlElements();
        BuildActors(mapComp);

        AddComponentToEntity(GetNewEntity(), new AdvanceClockEvent());
    }

    private void CreateSystems()
    {
        AddSystem(new ManualCameraControlSystem());
        AddSystem(new MouseToMapSystem());
        AddSystem(new PulseSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new RenderTurnOrderCardsSystem());
        AddSystem(new RemoveDyingEntitiesSystem());
        AddSystem(new TweenCleanupSystem());
        AddSystem(new RenderStatusEffectsSystem());

        // Event Handling Systems
        AddSystem(new SelectActionEventSystem());
        AddSystem(new AdvanceClockEventSystem());
        AddSystem(new SetActionsDisplayStateEventSystem());
        AddSystem(new DeferredEventSystem());

        AddSystem<PlayerMovementState>(new TravelToLocationSystem());
        AddSystem<PlayerMovementState>(new RenderSelectedMovementSystem());

        AddSystem<PlayerTargetingState>(new SelectActionLocationSystem());
        AddSystem<PlayerTargetingState>(new RenderTargetIndicatorsSystem());
        AddSystem<PlayerTargetingState>(new RenderTargetProfileSystem());

        AddSystem<RoamMapState>(new SelectActorSystem());
    }

    private Map BuildMap()
    {
        var mapNode = FindNode("Map");
        var tileToTerrain = new Dictionary<int, TerrainType>()
        {
            { 0, TerrainType.Dirt },
            { 1, TerrainType.Dirt },
            { 2, TerrainType.Dirt },
            { 3, TerrainType.Grass },
            { 4, TerrainType.Grass },
            { 5, TerrainType.Grass },
            { 6, TerrainType.Grass },
            { 7, TerrainType.Stone },
            { 8, TerrainType.Stone },
            { 9, TerrainType.Stone },
            { 10, TerrainType.Stone },
            { 11, TerrainType.Water },
            { 12, TerrainType.Water },
            { 13, TerrainType.DeepWater },
            { 14, TerrainType.Sand },
            { 15, TerrainType.Sand },
            { 16, TerrainType.Wood },
            { 17, TerrainType.Wood },
            { 18, TerrainType.Wood },
            { 19, TerrainType.Wood },
        };

        var tileEntities = new List<Entity>();
        var height = 0;
        foreach (var child in mapNode.GetChildren())
        {
            if (child is TileMap map)
            {
                var tileSet = map.TileSet;
                foreach (Vector2 tile in map.GetUsedCells())
                {
                    var tileId = map.GetCellv(tile);

                    var tileEnt = GetNewEntity();
                    AddComponentsToEntity(tileEnt, 
                        new TileLocation() { TilePosition = new Vector3(tile.x, tile.y, height), ZLayer = 0 }, 
                        new SpriteWrap(), 
                        new Terrain() { Type = tileToTerrain[tileId] });
                    tileEntities.Add(tileEnt);

                    tileEnt.GetComponent<SpriteWrap>().Sprite.Texture = map.TileSet.TileGetTexture(tileId);
                }

                height++;
            }
        }

        var mapEnt = GetNewEntity();
        var mapComp = new Map(tileEntities);
        AddComponentToEntity(mapEnt, mapComp);

        mapNode.QueueFree();

        return mapComp;
    }

    private void BuildControlElements()
    {
        var camera = FindNode("Camera") as Entity;
        RegisterExistingEntity(camera);
        AddComponentToEntity(camera, new CameraWrap());

        var target = FindNode("Target") as Entity;
        RegisterExistingEntity(target);
        AddComponentsToEntity(target,
            new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 5 },
            new SpriteWrap(), 
            new Reticle(), 
            new CameraRef() { Camera = camera.GetComponent<CameraWrap>().Camera }, 
            new TileLocation() { ZLayer = 2 });
    }

    private Vector3 TilePositionFromActor(Entity actor, Map map) =>
        map.IsoMap.PickUncovered(actor.Position)[0].GetComponent<TileLocation>().TilePosition;

    private void BuildActors(Map map)
    {
        var flyingMoveType = new Dictionary<TerrainType, float>
        {
            { TerrainType.Stone, 1 },
            { TerrainType.Wood, 1 },
            { TerrainType.Grass, 1 },
            { TerrainType.Dirt, 1 },
            { TerrainType.Water, 1 },
            { TerrainType.DeepWater, 1 },
            { TerrainType.Sand, 1 },
        };

        var amphibiousMoveType = new Dictionary<TerrainType, float>
        {
            { TerrainType.Water, 1 },
            { TerrainType.DeepWater, 1 },
        };
        var skillList = new List<Skill>()
        {
            new Skill() { Name = "Tackle", Speed = 5, MaxTP = 999, CurrentTP = 500, MinRange = 1, MaxRange = 1, Accuracy = 95, Effects = new Dictionary<string, int>() { { "StrDamage", 10 } } },
            new Skill() { Name = "Throw Bomb", Speed = 8, MaxTP = 10, CurrentTP = 10, AreaOfEffect = 1, MaxAoeHeightDelta = 1, MinRange = 2, MaxRange = 5, Accuracy = 60, Effects = new Dictionary<string, int>() { { "MagDamage", 30 } } },
            new Skill() { Name = "Double Team", Speed = 3, MaxTP = 8, CurrentTP = 8, MinRange = 0, MaxRange = 0, Accuracy = 9999, Effects = new Dictionary<string, int>() { { "Haste", 3 } } },
            //new Skill() { Name = "Heal", Speed = 6, MaxTP = 5, CurrentTP = 5, MinRange = 0, MaxRange = 2, Accuracy = 9999, Effects = new Dictionary<string, int>() { { "Heal", 20 } } },
        };

        var actor = FindNode("Vaporeon") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new ProfileDetails() { Name = "Vaporeon", MonNumber = 134, Affiliation = Affiliation.Friendly },
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 10 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new PlayerCharacter(), 
            new Health() { Current = 30, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 2, TerrainCostModifiers = amphibiousMoveType, TravelSpeed = 3 }, 
            new TurnSpeed() { TimeToAct = 12 },
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Water },
            new StatusBag(),
            new SkillSet() { Skills = skillList });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new ProfileDetails() { Name = "Scyther", MonNumber = 123, Affiliation = Affiliation.Friendly },
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 10 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new PlayerCharacter(), 
            new Health() { Current = 10, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 2, TravelSpeed = 2 }, 
            new TurnSpeed() { TimeToAct = 16 },
            new FightStats() { Atn = 5, Dex = 17, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Earth },
            new StatusBag(),
            new SkillSet() { Skills = skillList });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Zapdos") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new ProfileDetails() { Name = "Zapdos", MonNumber = 145, Affiliation = Affiliation.Enemy },
            new Pulse() { squishAmountY = 0.03f, squishSpeed = 2 }, 
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(),
            new EnemyNpc(),
            new Health() { Current = 24, Max = 30 }, 
            new Movable() { MaxMove = 2, MaxJump = 1, TerrainCostModifiers = flyingMoveType, TravelSpeed = 2 }, 
            new TurnSpeed() { TimeToAct = 14 },
            new StatusBag(),
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Fire },
            new SkillSet() { Skills = skillList });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Machamp") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new ProfileDetails() { Name = "Machamp", MonNumber = 68, Affiliation = Affiliation.Enemy },
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new EnemyNpc(), 
            new Health() { Current = 30, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 2, TravelSpeed = 4 }, 
            new TurnSpeed() { TimeToAct = 26 },
            new StatusBag(),
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Neutral },
            new SkillSet() { Skills = skillList });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Rock") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 3 }, 
            new SpriteWrap(), 
            new Obstacle(), 
            new Selectable(), 
            new Health() { Current = 30, Max = 30 },
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 99 });
    }

    public override void PerformHudAction(string actionName, params object[] args)
    {
        switch (actionName)
        {
            case "SetProfile":
                var side = (Direction)args[0];
                var profileEntity = args[1] as Entity;
                if (side == Direction.Left)
                {
                    leftProfileCard.SetProfile(profileEntity);
                }
                else if (side == Direction.Right)
                {
                    rightProfileCard.SetProfile(profileEntity);
                }
                break;
            case "SetActionMenuDisplayed":
                if ((bool)args[0])
                {
                    leftProfileCard.MakeRoomForActionMenu();
                }
                else
                {
                    leftProfileCard.TakeAwayRoomForActionMenu();
                }
                break;
            case "SetActions":
                actionMenu.RegisterSkillSet(args[0] as SkillSet, (bool)args[1]);
                break;
            case "SetTargetingInfo":
                rightProfileCard.TargetingInfo = args[0] as string;
                break;
            case "UpdateStatusEffects":
                {
                    var entity = args[0] as Entity;
                    if (leftProfileCard.MatchesCurrentEntity(entity))
                    {
                        leftProfileCard.SetStatusEffects(entity.GetComponent<StatusBag>());
                    }
                    else if (rightProfileCard.MatchesCurrentEntity(entity))
                    {
                        rightProfileCard.SetStatusEffects(entity.GetComponent<StatusBag>());
                    }
                }
                break;
            case "FlashMove":
                leftProfileCard.FlashMove((int)args[0]);
                break;
            default:
                throw new ArgumentException($"Attempting to perform an illegal HUD action: {actionName}");
        }
    }
}
