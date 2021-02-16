using Godot;
using Ecs;
using System.Collections.Generic;

public class Combat : Manager
{
    public override void _Ready()
    {
        CreateSystems();
        BuildMap();
        BuildReticle();
        BuildActors();
    }

    private void CreateSystems()
    {
        AddSystem(new MouseToMapSystem());
        AddSystem(new PulseSystem());
        AddSystem(new ClampToMapSystem());
        // TTLSystem needs to be before SelectActorSystem
        AddSystem(new TravelToLocationSystem());
        AddSystem(new SelectActorSystem());
        AddSystem(new RenderSelectedStatsSystem());
        AddSystem(new RefreshObstaclesSystem());
        AddSystem(new RenderSelectedMovementSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new RenderTurnOrderCardsSystem());

        // Event Handling Systems
        AddSystem(new AdvanceClockEventSystem());
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

    private void BuildReticle()
    {
        var camera = FindNode("Camera2D") as Camera2D;
        var target = FindNode("Target") as Entity;
        RegisterExistingEntity(target);
        AddComponentsToEntity(target,
            new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 5 },
            new SpriteWrap(), 
            new Reticle(), 
            new CameraRef() { Camera = camera }, 
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

        var actor = FindNode("Vaporeon") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new TileLocation() { TilePosition = new Vector3(6, 3, 0), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new FriendlyNpc(), 
            new Health() { Current = 30, Max = 30 }, 
            new Mana() { Current = 20, Max = 20 }, 
            new CombatStats() { Attack = 4, Defense = 3 }, 
            new Movable() { MaxMove = 4, MaxJump = 2, TerrainCostModifiers = amphibiousMoveType }, 
            new TurnSpeed() { Speed = 16, TimeToAct = 16 },
            TurnOrderCard.For("134", Affiliation.Friendly));

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new TileLocation() { TilePosition = new Vector3(12, -2, 0), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new PlayerCharacter(), 
            new Health() { Current = 30, Max = 30 }, 
            new Mana() { Current = 20, Max = 20 }, 
            new CombatStats() { Attack = 4, Defense = 3 }, 
            new Movable() { MaxMove = 4, MaxJump = 2 }, 
            new TurnSpeed() { Speed = 12, TimeToAct = 12 },
            TurnOrderCard.For("123", Affiliation.Friendly));

        actor = FindNode("Zapdos") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new Pulse() { squishAmountY = 0.03f, squishSpeed = 2 }, 
            new TileLocation() { TilePosition = new Vector3(4, 0, 3), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new EnemyNpc(), 
            new Health() { Current = 30, Max = 30 }, 
            new Mana() { Current = 20, Max = 20 }, 
            new CombatStats() { Attack = 4, Defense = 3 }, 
            new Movable() { MaxMove = 4, MaxJump = 10, TerrainCostModifiers = flyingMoveType }, 
            new TurnSpeed() { Speed = 14, TimeToAct = 14 },
            TurnOrderCard.For("145", Affiliation.Enemy));

        actor = FindNode("Machamp") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new TileLocation() { TilePosition = new Vector3(6, -1, 1), ZLayer = 5 }, 
            new SpriteWrap(), 
            new Selectable(), 
            new EnemyNpc(), 
            new Health() { Current = 30, Max = 30 }, 
            new Mana() { Current = 20, Max = 20 }, 
            new CombatStats() { Attack = 4, Defense = 3 }, 
            new Movable() { MaxMove = 4, MaxJump = 2 }, 
            new TurnSpeed() { Speed = 26, TimeToAct = 26 },
            TurnOrderCard.For("68", Affiliation.Enemy));

        actor = FindNode("Rock") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor, 
            new TileLocation() { TilePosition = new Vector3(9, 2, 0), ZLayer = 3 }, 
            new SpriteWrap(), 
            new Obstacle(), 
            new Selectable(), 
            new Health() { Current = 30, Max = 30 }, 
            new CombatStats() { Attack = -1, Defense = 99 });
    }
}
