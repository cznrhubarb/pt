using Ecs;
using Godot;

public class NpcTurnState : State
{
    public Entity Acting { get; set; }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC Acting: " + Acting.Name);

        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += (s, e) => manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
        timer.AutoReset = false;
        timer.Enabled = true;
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC Done acting: " + Acting.Name);
    }
}