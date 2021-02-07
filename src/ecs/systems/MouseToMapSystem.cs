using Ecs;
using Godot;
using System.Linq;

public class MouseToMapSystem : Ecs.DyadicSystem
{
    private Vector2 mousePosition;

    public MouseToMapSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<CameraRef>();
        AddRequiredSecondaryComponent<Map>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var tileLocationComp = entity.GetComponent<TileLocation>();
        var cameraComp = entity.GetComponent<CameraRef>();
        var tilemaps = SecondaryEntities.First().GetComponent<Map>().TileMaps;
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
