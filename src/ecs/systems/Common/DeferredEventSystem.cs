using Ecs;

public class DeferredEventSystem : Ecs.System
{
    public DeferredEventSystem()
    {
        AddRequiredComponent<DeferredEvent>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var deferredEvent = entity.GetComponent<DeferredEvent>();
        deferredEvent.Delay -= deltaTime;
        if (deferredEvent.Delay <= 0)
        {
            deferredEvent.Callback.Invoke();
            manager.DeleteEntity(entity.Id);
        }
    }
}
