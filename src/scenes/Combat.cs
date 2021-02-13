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
        AddSystem(new RenderSelectedMovementSystem());
        AddSystem(new DepthSortSystem());
    }

    private void BuildMap()
    {
        var mapNode = FindNode("Map");

        var tileEntities = new List<Entity>();
        var height = 0;
        foreach (var child in mapNode.GetChildren())
        {
            if (child is TileMap map)
            {
                var tileSet = map.TileSet;
                foreach (Vector2 tile in map.GetUsedCells())
                {
                    var tileEnt = GetNewEntity();
                    AddComponentToEntity(tileEnt, new TileLocation() { TilePosition = new Vector3(tile.x, tile.y, height), ZLayer = 0 });
                    AddComponentToEntity(tileEnt, new SpriteWrap());
                    AddComponentToEntity(tileEnt, new Terrain());
                    tileEntities.Add(tileEnt);

                    tileEnt.GetComponent<SpriteWrap>().Sprite.Texture = map.TileSet.TileGetTexture(map.GetCellv(tile));
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
        var actor = FindNode("Vaporeon") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(11, 3, 0), ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new PlayerCharacter());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new Movable() { MaxMove = 4, MaxJump = 2 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector3(4, -2, 0), ZLayer = 5 });
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
        AddComponentToEntity(actor, new Movable() { MaxMove = 4, MaxJump = 10 });
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
