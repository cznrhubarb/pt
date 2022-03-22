using Ecs;
using Godot;

public class CSETurnActorToTarget : CutSceneEvent
{
    public NodePath ActorPath { get; set; } = null;
    public NodePath TargetPath { get; set; } = null;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        var target = Manager.GetNode(TargetPath) as Entity;
        var actorPosition = actor.GetComponent<TileLocation>().TilePosition;
        var targetPosition = target.GetComponent<TileLocation>().TilePosition;

        actor.GetComponent<Directionality>().Direction = (targetPosition - actorPosition).ToDirection();
        OnComplete();
    }
}