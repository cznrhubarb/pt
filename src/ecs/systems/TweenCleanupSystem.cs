using Ecs;

public class TweenCleanupSystem : Ecs.System
{
    public TweenCleanupSystem()
    {
        AddRequiredComponent<Tweening>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var tweening = entity.GetComponent<Tweening>();
        if (tweening.TweenSequence.Complete)
        {
            manager.RemoveComponentFromEntity<Tweening>(entity);
            if (tweening.DeleteEntityOnComplete)
            {
                manager.DeleteEntity(entity.Id);
            }
        }
    }
}
