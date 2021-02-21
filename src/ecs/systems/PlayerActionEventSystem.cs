using Ecs;
using Godot;

public class PlayerActionEventSystem : Ecs.System
{

    public PlayerActionEventSystem()
    {
        AddRequiredComponent<ActionMenu>();
    }

    protected override void Update(Entity entity, float deltaTime) 
    {
        var menuComp = entity.GetComponent<ActionMenu>();
        if (menuComp.SelectedMenuAction != null)
        {
            // TODO: Need to also do the action obviously
            // TODO: Decrement TP of used skill
            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
            menuComp.SelectedMenuAction = null;
        }
    }
}
