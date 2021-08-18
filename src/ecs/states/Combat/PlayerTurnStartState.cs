using Ecs;
using Godot;
using System.Linq;

public class PlayerTurnStartState : State
{
    private Entity acting;
    private Entity map;

    public PlayerTurnStartState(Entity acting, Entity map)
    {
        this.acting = acting;
        this.map = map;
    }

    public override void Pre(Manager manager)
    {
        manager.PerformHudAction("SetProfile", Direction.Left, acting);
        manager.PerformHudAction("SetProfile", Direction.Right, null);

        if (acting.GetComponent<StatusBag>().Statuses.ContainsKey("Sleep"))
        {
            acting.GetComponent<TurnSpeed>().TimeToAct = 40;
            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
            return;
        }

        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () => manager.ApplyState(new PlayerMovementState(acting, map))
        });
    }

    public override void Post(Manager manager)
    {
    }
}
