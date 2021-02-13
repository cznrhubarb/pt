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
                    AddComponentToEntity(tileEnt, new TileLocation() { TilePosition = new Vector3(tile.x, tile.y, height), ZLayer = 0 });
                    AddComponentToEntity(tileEnt, new SpriteWrap());
                    AddComponentToEntity(tileEnt, new Terrain() { Type = tileToTerrain[tileId] });
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
        AddComponentToEntity(target, new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 5 });
        AddComponentToEntity(target, new SpriteWrap());
        AddComponentToEntity(target, new Reticle());
        AddComponentToEntity(target, new CameraRef() { Camera = camera });
        AddComponentToEntity(target, new TileLocation() { ZLayer = 2 });
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
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(6, 3, 0), ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new FriendlyNpc());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new Movable() { MaxMove = 4, MaxJump = 2, TerrainCostModifiers = amphibiousMoveType });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(12, -2, 0), ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new PlayerCharacter());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new Movable() { MaxMove = 4, MaxJump = 2 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Zapdos") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new Pulse() { squishAmountY = 0.03f, squishSpeed = 2 });
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(4, 0, 3), ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new EnemyNpc());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new Movable() { MaxMove = 4, MaxJump = 10, TerrainCostModifiers = flyingMoveType });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Machamp") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(6, -1, 1), ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new EnemyNpc());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new Movable() { MaxMove = 4, MaxJump = 2 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Rock") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(9, 2, 0), ZLayer = 3 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Obstacle());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new CombatStats() { Attack = -1, Defense = 99 });
    }
}
