using Ecs;
using Godot;

public class MouseToMapSystem : Ecs.System
{
    private Vector2 mousePosition;

    public MouseToMapSystem()
    {
        AddRequiredComponent<FollowMouse>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<CameraRef>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var tileLocationComp = entity.GetComponent<TileLocation>();
        var cameraComp = entity.GetComponent<CameraRef>();
        var tilemaps = tileLocationComp.MapRef.TileMaps;
        var translatedMousePos = mousePosition + cameraComp.Camera.Position;

        for (var i = 0; i < tilemaps.Count; i++)
        {
            var tilemap = tilemaps[i];

            var tilePos = tilemap.WorldToMap(translatedMousePos - tilemap.Position);
            if (tilemap.GetCell((int)tilePos.x, (int)tilePos.y) != TileMap.InvalidCell)
            {
                while (i > 0 && tilemaps[i - 1].GetCell((int)tilePos.x, (int)tilePos.y) != TileMap.InvalidCell)
                {
                    i--;
                }

                tileLocationComp.TilePosition = tilePos;
                tileLocationComp.Height = i;
                break;
            }
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        base._Input(inputEvent);

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            mousePosition = mouseMotion.GlobalPosition;
        }
    }
}
