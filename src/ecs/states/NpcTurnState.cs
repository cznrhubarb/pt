using Ecs;
using Godot;

public class NpcTurnState : State
{
    private Entity acting;

    public NpcTurnState(Entity acting)
    {
        this.acting = acting;
    }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC Acting: " + acting.Name);
        // TODO: Need another state for preview movement of entity
        manager.PerformHudAction("SetProfile", Direction.Left, acting);
        manager.PerformHudAction("SetProfile", Direction.Right, null);

        // TODO: Placeholder until AI is in.
        acting.GetComponent<TurnSpeed>().TimeToAct += 35;

        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += (s, e) => manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
        timer.AutoReset = false;
        timer.Enabled = true;
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC Done acting: " + acting.Name);
    }
}