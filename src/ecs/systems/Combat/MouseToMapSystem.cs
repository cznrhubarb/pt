using Ecs;
using Godot;

public class MouseToMapSystem : Ecs.System
{
    private const string MapEntityKey = "map";

    private Vector2 mousePosition;
    private bool dirty = false;

    public MouseToMapSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<CameraRef>();
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        if (dirty)
        {
            var tileLocationComp = entity.GetComponent<TileLocation>();
            var camera = entity.GetComponent<CameraRef>().Camera;
            var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
            var translatedMousePos = mousePosition + camera.Position - GetViewport().Size/2;

            var pickedTiles = map.IsoMap.PickUncovered(translatedMousePos);
            if (pickedTiles.Count != 0)
            {
                tileLocationComp.TilePosition = pickedTiles[0].GetComponent<TileLocation>().TilePosition;
            }

            dirty = false;
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        base._Input(inputEvent);

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            mousePosition = mouseMotion.GlobalPosition;
            dirty = true;
        }
    }
}
