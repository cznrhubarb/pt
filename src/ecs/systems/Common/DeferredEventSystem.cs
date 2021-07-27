using Ecs;

public class DeferredEventSystem : Ecs.System
{
    public DeferredEventSystem()
    {
        AddRequiredComponent<DeferredEvent>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        entity.GetComponent<DeferredEvent>().Callback.Invoke();
        manager.DeleteEntity(entity.Id);
    }
}
