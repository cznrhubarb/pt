using Ecs;
using Godot;
using System.Linq;

public class CSEMoveActorToTarget : CutSceneEvent
{
    [Export]
    public NodePath ActorPath { get; set; } = null;
    [Export]
    public NodePath TargetPath { get; set; } = null;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        var target = Manager.GetNode(TargetPath) as Entity;
        var actorLocation = actor.GetComponent<TileLocation>();
        var finalPosition = target.GetComponent<TileLocation>().TilePosition;

        if (actorLocation.TilePosition != finalPosition)
        {
            var map = Manager.GetEntitiesWithComponent<Map>().First().GetComponent<Map>();
            var path = map.AStar.GetPath(null, actorLocation.TilePosition, finalPosition);

            var tweenSeq = MapUtils.BuildTweenForActor(Manager, actor, path);
            tweenSeq.Connect("finished", this, nameof(MovementFinished));
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
