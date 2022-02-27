using Godot;
using Ecs;
using System.Collections.Generic;
using System;
using System.Linq;

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

        AddComponentToEntity(GetNewEntity(), new CombatSpoils());

        AnchorCamera();

        AddComponentToEntity(GetNewEntity(), new AdvanceClockEvent());
    }

    private void CreateSystems()
    {
        AddSystem(new ManualCameraControlSystem());
        AddSystem(new MouseToMapSystem());
        AddSystem(new PulseSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new ApplyPositionOffsetSystem());
        AddSystem(new ApplyDirectionToSpriteSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new RenderTurnOrderCardsSystem());
        AddSystem(new RemoveDyingEntitiesSystem());
        AddSystem(new TweenCleanupSystem());
        AddSystem(new RenderStatusEffectsSystem());
        AddSystem(new CameraAnchoringSystem());

        // Event Handling Systems
        AddSystem(new CreateShockwaveEventSystem());
        AddSystem(new StatusTickEventSystem());
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

        // TODO: Move these components into the editor
        var target = FindNode("Target") as Entity;
        RegisterExistingEntity(target);
        AddComponentsToEntity(target,
            new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 5 },
            new SpriteWrap(), 
            new Reticle(), 
            new CameraRef() { Camera = camera.GetComponent<CameraWrap>().Camera }, 
            new TileLocation() { ZLayer = 2 });
        
        // HACK: All things inside Actors should be auto registered (like in Exploration), but right now not
        //  doing that because Target is being handled as a special case and it is in list.
        RegisterExistingEntity(FindNode("CamAnchor") as Entity);
    }

    private void BuildActors(Map map)
    {
        // HACK: Setting these for testing
        WorldState.PartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Bulbasaur.tres"), 1));
        WorldState.RivalPartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Charmander.tres"), 1));

        // HACK: Just hardcoding some starting points to get started
        var positions = new Queue<Vector3>();
        positions.Enqueue(new Vector3(5, 0, 2));
        positions.Enqueue(new Vector3(5, 1, 2));
        positions.Enqueue(new Vector3(11, 0, 0));
        positions.Enqueue(new Vector3(11, -1, 0));

        foreach (var state in WorldState.PartyState)
        {
            var components = MonsterFactory.GenerateComponents(state, Affiliation.Friendly);
            var actor = GetNewEntity();
            actor.AddChild(new Sprite() { Texture = state.Blueprint.Sprite, Position = new Vector2(0, -23), Hframes = 4 });
            AddComponentsToEntity(actor, components.ToArray());
            AddComponentsToEntity(actor,
                new TileLocation() { TilePosition = positions.Dequeue(), ZLayer = 10 },
                new SpriteWrap(),
                TurnOrderCard.For(actor.GetComponent<ProfileDetails>(), Affiliation.Friendly));
        }

        foreach (var state in WorldState.RivalPartyState)
        {
            var components = MonsterFactory.GenerateComponents(state, Affiliation.Enemy);
            var actor = GetNewEntity();
            actor.AddChild(new Sprite() { Texture = state.Blueprint.Sprite, Position = new Vector2(0, -23), Hframes = 4 });
            AddComponentsToEntity(actor, components.ToArray());
            AddComponentsToEntity(actor,
                new TileLocation() { TilePosition = positions.Dequeue(), ZLayer = 10 },
                new SpriteWrap(),
                TurnOrderCard.For(actor.GetComponent<ProfileDetails>(), Affiliation.Enemy));
        }
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
