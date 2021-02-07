using Ecs;
using Godot;

public class ClampToMapSystem : Ecs.System
{
    public ClampToMapSystem()
    {
        AddRequiredComponent<TileLocation>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var tileLocationComp = entity.GetComponent<TileLocation>();
        var tileMap = tileLocationComp.MapRef.TileMaps[tileLocationComp.Height];
        entity.Position = tileMap.MapToWorld(tileLocationComp.TilePosition) + new Vector2(0, 24) + tileMap.Position;
        entity.ZIndex = tileMap.ZIndex + tileLocationComp.ZLayer;
    }
}
