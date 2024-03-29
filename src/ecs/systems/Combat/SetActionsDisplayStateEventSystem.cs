using Ecs;

public class SetActionsDisplayStateEventSystem : Ecs.System
{

    public SetActionsDisplayStateEventSystem()
    {
        AddRequiredComponent<SetActionsDisplayStateEvent>();
    }

    protected override void Update(Entity entity, float deltaTime) 
    {
        manager.PerformHudAction("SetActionMenuDisplayed", entity.GetComponent<SetActionsDisplayStateEvent>().Displayed);
        manager.DeleteEntity(entity.Id);
    }
}
