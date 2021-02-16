using Ecs;

public class RenderSelectedStatsSystem : Ecs.System
{
    public RenderSelectedStatsSystem()
    {
        AddRequiredComponent<Selected>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        entity.GetComponentOrNull<TurnSpeed>();
    }
}
