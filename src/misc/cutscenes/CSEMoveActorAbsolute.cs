using Ecs;
using Godot;
using System.Linq;
using MonoCustomResourceRegistry;

// TODO: Clean up the fact that this is a 99% duplicate of CSEMoveActorToTarget
[RegisteredType(nameof(CSEMoveActorAbsolute), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
public class CSEMoveActorAbsolute : CutSceneEvent
{
    [Export]
    public NodePath ActorPath { get; set; } = null;
    [Export]
    public Vector3 FinalPosition { get; set; } = Vector3.Zero;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        var actorLocation = actor.GetComponent<TileLocation>();

        if (actorLocation.TilePosition != FinalPosition)
        {
            var map = Manager.GetEntitiesWithComponent<Map>().First().GetComponent<Map>();
            var path = map.AStar.GetPath(null, actorLocation.TilePosition, FinalPosition);

            var tweenSeq = MapUtils.BuildTweenForActor(Manager, actor, path);
            tweenSeq.Connect("finished", this, nameof(MovementFinished), new Godot.Collections.Array() { Manager, 0.5f });
        }
        else
        {
            MovementFinished();
        }
    }

    private void MovementFinished()
    {
        OnComplete();
    }
}
