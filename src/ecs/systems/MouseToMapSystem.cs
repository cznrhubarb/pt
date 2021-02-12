using Ecs;
using Godot;
using System.Linq;

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
            var cameraComp = entity.GetComponent<CameraRef>();
            var tilemaps = SingleEntityFor(MapEntityKey).GetComponent<Map>().TileMaps;
            var translatedMousePos = mousePosition + cameraComp.Camera.Position;

            // NEW_MAP
            //var pickedTiles = tileMap.PickUncovered(translatedMousePos);
            //if (!pickedTiles.IsEmpty)
            //{
            //  tileLocationComp.TilePosition = pickedTiles[0];
            //}

            // TODO: MAP
            for (var i = tilemaps.Count - 1; i >= 0; i--)
            {
                var tilemap = tilemaps[i];

                var tilePos = tilemap.WorldToMap(translatedMousePos - tilemap.Position);
                if (tilemap.GetCell((int)tilePos.x, (int)tilePos.y) != TileMap.InvalidCell)
                {
                    while (i < tilemaps.Count - 1 && tilemaps[i + 1].GetCell((int)tilePos.x, (int)tilePos.y) != TileMap.InvalidCell)
                    {
                        i++;
                    }

                //    tileLocationComp.TilePosition = tilePos;
                    break;
                }
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
