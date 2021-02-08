using Ecs;
using Godot;
using System.Linq;

public class ClampToMapSystem : Ecs.System
{
    const string MapEntityKey = "map";

    public ClampToMapSystem()
    {
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();

        var tileLocationComp = entity.GetComponent<TileLocation>();
        var tileMap = map.TileMaps[tileLocationComp.Height];
        entity.Position = tileMap.MapToWorld(tileLocationComp.TilePosition) + new Vector2(0, 24) + tileMap.Position;
        entity.ZIndex = tileMap.ZIndex + tileLocationComp.ZLayer;
    }
}
