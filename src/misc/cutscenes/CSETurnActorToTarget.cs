using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CSETurnActorToTarget), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
public class CSETurnActorToTarget : CutSceneEvent
{
    [Export]
    public NodePath ActorPath { get; set; } = null;
    [Export]
    public NodePath TargetPath { get; set; } = null;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        var target = Manager.GetNode(TargetPath) as Entity;
        GD.Print("TURN TO TARGET");
        var actorPosition = actor.GetComponent<TileLocation>().TilePosition;
        var targetPosition = target.GetComponent<TileLocation>().TilePosition;

        actor.GetComponent<Directionality>().Direction = (targetPosition - actorPosition).ToDirection();
        OnComplete();
    }
}