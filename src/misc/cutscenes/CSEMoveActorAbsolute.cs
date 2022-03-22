using Ecs;
using Godot;
using System.Linq;
using System.Collections.Generic;

public class CSEMoveActorAbsolute : CutSceneEvent
{
    private static readonly Movable cutSceneMovable = new Movable() 
    { 
        MaxJump = 2, 
        MaxMove = 99, 
        TerrainCostModifiers = new Dictionary<TerrainType, float>
        {
            { TerrainType.Water, 99 },
            { TerrainType.DeepWater, 99 },
        }
    };

    public NodePath ActorPath { get; set; } = null;
    public Vector3 FinalPosition { get; set; } = Vector3.Zero;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        var actorLocation = actor.GetComponent<TileLocation>();

        if (actorLocation.TilePosition != FinalPosition)
        {
            var map = Manager.GetEntitiesWithComponent<Map>().First().GetComponent<Map>();
            var path = map.AStar.GetPath(cutSceneMovable, Affiliation.Neutral, actorLocation.TilePosition, FinalPosition);

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
