using Ecs;
using Godot;

public class NpcTurnState : State
{
    public Entity Acting { get; set; }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC Acting: " + Acting.Name);
        // TODO: Need another state for preview movement of entity
        // TODO: Move this code to a centralized location used by both states
        // TODO: All of the 'as Combat' things should be revisited. We could add a "HudAction" method to the manager
        //  class and pass a varargs parameter and then not have to dynamically cast this each time and also make
        //  it available to other Manager/scenes
        (manager as Combat).SetProfile(Direction.Left, Acting);
        (manager as Combat).SetProfile(Direction.Right, null);

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