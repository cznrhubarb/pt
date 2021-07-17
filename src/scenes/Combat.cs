using Godot;
using Ecs;
using System.Collections.Generic;

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
        actionMenu.SetButtonCallback(move => AddComponentToEntity(GetNewEntity(), new SelectActionEvent() { SelectedMove = move }));

        ApplyState(new CombatStartState());
        CreateSystems();
        BuildMap();
        BuildControlElements();
        BuildActors();

        AddComponentToEntity(GetNewEntity(), new AdvanceClockEvent());


        // Game State flow
        //  Player movement
        //      Selected player
        //      Potential movement locations
        //          Render movement location
        //          Allow clicking of movement locations
        //  Player action selection
        //      Selected player
        //      Selected player's moveset
        //          Render action menu
        //          Allow clicking of moveset options
        //  Player action targeting
        //      Selected player
        //      Selected move's range and target area
        //      Entities within target area
        //          Render target range
        //          Render target area
        //          Click to target
        //          Confirmation
        //  Player action execution
        //      Selected player
        //      Selected targets
        //      Selected move
        //          Render and yield for animations
        //  NPC AI
        //      Most of the world state
        //          Choose an option
        //  NPC movement, action selection, targeting, execution (maybe can be all one state?)
        //  
        //  Player movement, action selection, action targeting could all switch to Free Roam and back
        //  Or maybe free roam isn't a thing? Maybe if you are in any other state that allow cursor movement we just render state under cursor?
        //      Previous state
        //      Entity state
        //      Terrain state
        //          Render stats for entity under cursor
        //          Allow returning to previous state
        //
        //
        //  In order to go back to movement state for player, we need to store starting location the entire time
        //      (until the next movement state for that player? or clear it when execution is over?)
    }

    private void CreateSystems()
    {
        AddSystem(new CameraControlSystem());
        AddSystem(new MouseToMapSystem());
        AddSystem(new PulseSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new RenderSelectedStatsSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new RenderTurnOrderCardsSystem());

        // Event Handling Systems
        AddSystem(new SelectActionEventSystem());
        AddSystem(new AdvanceClockEventSystem());
        AddSystem(new SetActionsDisplayStateEventSystem());

        AddSystem<PlayerMovementState>(new TravelToLocationSystem());
        AddSystem<PlayerMovementState>(new RenderSelectedMovementSystem());

        AddSystem<PlayerTargetingState>(new SelectActionLocationSystem());
        AddSystem<PlayerTargetingState>(new RenderTargetIndicatorsSystem());
        AddSystem<PlayerTargetingState>(new RenderTargetProfileSystem());

        AddSystem<RoamMapState>(new SelectActorSystem());
    }

    private void BuildMap()
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
        AddComponentToEntity(mapEnt, new Map(tileEntities));

        mapNode.QueueFree();
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

    private void BuildActors()
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
        var moveList = new List<Move>()
        {
            new Move() { Name = "Tackle", MaxTP = 999, CurrentTP = 999, MinRange = 1, MaxRange = 1, Accuracy = 95, Effects = new Dictionary<string, int>() { { "StrDamage", 10 } } },
            new Move() { Name = "Throw Bomb", MaxTP = 10, CurrentTP = 10, AreaOfEffect = 1, MaxAoeHeightDelta = 1, MinRange = 2, MaxRange = 5, Accuracy = 60, Effects = new Dictionary<string, int>() { { "MagDamage", 30 } } },
            new Move() { Name = "Double Team", MaxTP = 8, CurrentTP = 8, MinRange = 0, MaxRange = 0, Accuracy = 9999, Effects = new Dictionary<string, int>() { { "StrengthBoost", 3 } } },
            new Move() { Name = "Heal", MaxTP = 5, CurrentTP = 5, MinRange = 0, MaxRange = 2, Accuracy = 9999, Effects = new Dictionary<string, int>() { { "Heal", 20 } } },
        };

        var actor = FindNode("Vaporeon") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new ProfileDetails() { Name = "Vaporeon", MonNumber = 134, Affiliation = Affiliation.Friendly },
            new TileLocation() { TilePosition = new Vector3(6, 3, 0), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new PlayerCharacter(), 
            new Health() { Current = 30, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 2, TerrainCostModifiers = amphibiousMoveType }, 
            new TurnSpeed() { Speed = 16, TimeToAct = 16 },
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Water },
            new MoveSet() { Moves = moveList });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new ProfileDetails() { Name = "Scyther", MonNumber = 123, Affiliation = Affiliation.Friendly },
            new TileLocation() { TilePosition = new Vector3(12, -2, 0), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new PlayerCharacter(), 
            new Health() { Current = 10, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 2 }, 
            new TurnSpeed() { Speed = 12, TimeToAct = 12 },
            new FightStats() { Atn = 5, Dex = 17, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Earth },
            new MoveSet() { Moves = moveList });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Zapdos") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new ProfileDetails() { Name = "Zapdos", MonNumber = 145, Affiliation = Affiliation.Enemy },
            new Pulse() { squishAmountY = 0.03f, squishSpeed = 2 }, 
            new TileLocation() { TilePosition = new Vector3(4, 0, 3), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(),
            new EnemyNpc(),
            new Health() { Current = 24, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 10, TerrainCostModifiers = flyingMoveType }, 
            new TurnSpeed() { Speed = 14, TimeToAct = 14 },
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Fire });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Machamp") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new ProfileDetails() { Name = "Machamp", MonNumber = 68, Affiliation = Affiliation.Enemy },
            new TileLocation() { TilePosition = new Vector3(6, -1, 1), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new EnemyNpc(), 
            new Health() { Current = 30, Max = 30 }, 
            new Movable() { MaxMove = 4, MaxJump = 2 }, 
            new TurnSpeed() { Speed = 26, TimeToAct = 26 },
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 9 },
            new Elemental() { Element = Element.Neutral });
        AddComponentToEntity(actor, TurnOrderCard.For(actor.GetComponent<ProfileDetails>()));

        actor = FindNode("Rock") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new TileLocation() { TilePosition = new Vector3(9, 2, 0), ZLayer = 3 }, 
            new SpriteWrap(), 
            new Obstacle(), 
            new Selectable(), 
            new Health() { Current = 30, Max = 30 },
            new FightStats() { Atn = 5, Dex = 7, Mag = 8, Str = 6, Tuf = 99 });
    }

    public void SetProfile(Direction side, Entity profileEntity)
    {
        if (side == Direction.Left)
        {
            leftProfileCard.SetProfile(profileEntity);
        }
        else if (side == Direction.Right)
        {
            rightProfileCard.SetProfile(profileEntity);
        }
    }

    public void SetActionMenuDisplayed(bool displayed)
    {
        if (displayed)
        {
            leftProfileCard.MakeRoomForActionMenu();
        }
        else
        {
            leftProfileCard.TakeAwayRoomForActionMenu();
        }
    }

    public void SetActions(MoveSet moveSet)
    {
        actionMenu.RegisterMoveSet(moveSet);
    }
}
