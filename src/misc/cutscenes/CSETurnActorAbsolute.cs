using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CSETurnActorAbsolute), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
public class CSETurnActorAbsolute : CutSceneEvent
{
    [Export]
    public NodePath ActorPath { get; set; } = null;
    [Export]
    public Direction Direction { get; set; } = Direction.Up;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        GD.Print("TURN ABSOLUTE");
        actor.GetComponent<Directionality>().Direction = Direction;
        OnComplete();
    }
}
