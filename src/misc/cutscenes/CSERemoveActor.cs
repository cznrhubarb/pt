using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CSERemoveActor), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
public class CSERemoveActor : CutSceneEvent
{
    [Export]
    public NodePath ActorPath { get; set; } = null;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        Manager.DeleteEntity(actor.Id);

        OnComplete();
    }
}