using Ecs;
using System;

public class DepthSortSystem : Ecs.System
{
    private const int SpaceFactor = 10;

    public DepthSortSystem()
    {
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<SpriteWrap>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var locationComp = entity.GetComponent<TileLocation>();

        // TODO: If we're tweening, this is still going based off the destination location's z and not the current tween pos :(
        entity.ZIndex = Convert.ToInt32((locationComp.TilePosition.x + locationComp.TilePosition.y + locationComp.TilePosition.z) * SpaceFactor) + locationComp.ZLayer;
    }
}
