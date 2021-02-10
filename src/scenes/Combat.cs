using Godot;
using Ecs;

public class Combat : Manager
{
    public override void _Ready()
    {
        base._Ready();

        AddSystem(new MouseToMapSystem());
        AddSystem(new PulseSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new SelectActorSystem());
        AddSystem(new RenderSelectedStatsSystem());
        AddSystem(new RenderSelectedMovementSystem());

        var camera = FindNode("Camera2D") as Camera2D;

        var map = FindNode("Map") as Entity;
        RegisterExistingEntity(map);
        var mapComponent = new Map();
        AddComponentToEntity(map, mapComponent);

        var target = FindNode("Target") as Entity;
        RegisterExistingEntity(target);
        AddComponentToEntity(target, new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 5 });
        AddComponentToEntity(target, new SpriteWrap());
        AddComponentToEntity(target, new Reticle());
        AddComponentToEntity(target, new CameraRef() { Camera = camera });
        AddComponentToEntity(target, new TileLocation() { ZLayer = 2 });

        var actor = FindNode("Vaporeon") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(11,3), Height = 0, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new PlayerCharacter());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new MoveStats() { Move = 4, Jump = 2 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(9, -4), Height = 0, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new PlayerCharacter());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new MoveStats() { Move = 4, Jump = 2 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Zapdos") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new Pulse() { squishAmountY = 0.03f, squishSpeed = 2 });
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(4, 0), Height = 3, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new EnemyNpc());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new MoveStats() { Move = 4, Jump = 10 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Machamp") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(12, 0), Height = 0, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new EnemyNpc());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new Mana() { Current = 20, Max = 20 });
        AddComponentToEntity(actor, new CombatStats() { Attack = 4, Defense = 3 });
        AddComponentToEntity(actor, new MoveStats() { Move = 4, Jump = 2 });
        AddComponentToEntity(actor, new Speed() { Value = 5 });

        actor = FindNode("Rock") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(9, 2), Height = 0, ZLayer = 3 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Obstacle());
        AddComponentToEntity(actor, new Selectable());
        AddComponentToEntity(actor, new Health() { Current = 30, Max = 30 });
        AddComponentToEntity(actor, new CombatStats() { Attack = -1, Defense = 99 });
    }
}
