using Ecs;
using Godot;

public class NpcPlanningState : State
{
    private Entity acting;
    private Map map;

    public NpcPlanningState(Entity acting, Entity mapEnt)
    {
        this.acting = acting;
        this.map = mapEnt.GetComponent<Map>();
    }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC planning: " + acting.Name);
        manager.PerformHudAction("SetProfile", Direction.Left, acting);
        manager.PerformHudAction("SetProfile", Direction.Right, null);

        acting.GetComponent<TurnSpeed>().TimeToAct = 20;

        // TODO: If planning takes any reasonable amount of time, show a "thinking" animation

        // Currently returning a MOCK
        var plan = Tactician.GetTurnPlan(acting);

        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () => manager.ApplyState(new NpcMovementState(acting, map, plan))
        });
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC done planning: " + acting.Name);
    }
}