using Ecs;
using Godot;

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
        // If we're tweening, assume that is handling our positioning.
        //  In the future, this potentially might change to some sort of "OverrideRender" component.
        if (entity.HasComponent<Tweening>())
        {
            return;
        }

        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();

        var tileLocationComp = entity.GetComponent<TileLocation>();
        entity.Position = map.IsoMap.MapToWorld(tileLocationComp.TilePosition) + new Vector2(0, 24);
    }
}
