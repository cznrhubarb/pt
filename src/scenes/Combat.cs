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
        AddComponentToEntity(target, new TileLocation() { ZLayer = 1 });

        var actor = FindNode("Vaporeon") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(11,3), Height = 3, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());

        actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(9, -4), Height = 3,ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());

        actor = FindNode("Zapdos") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new Pulse() { squishAmountY = 0.03f, squishSpeed = 2 });
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(4, 0), Height = 0, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());

        actor = FindNode("Machamp") as Entity;
        RegisterExistingEntity(actor);
        AddComponentToEntity(actor, new TileLocation() { TilePosition = new Vector2(12, 0), Height = 3, ZLayer = 5 });
        AddComponentToEntity(actor, new SpriteWrap());
        AddComponentToEntity(actor, new Selectable());
    }
}
