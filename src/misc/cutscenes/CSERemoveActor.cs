using Ecs;
using Godot;

public class CSERemoveActor : CutSceneEvent
{
    public NodePath ActorPath { get; set; } = null;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        Manager.DeleteEntity(actor.Id);

        OnComplete();
    }
}