﻿using Ecs;
using Godot;
using System.Linq;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

[RegisteredType(nameof(CSEMoveActorToTarget), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
public class CSEMoveActorToTarget : CutSceneEvent
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
            var path = map.AStar.GetPath(cutSceneMovable, Affiliation.Neutral, actorLocation.TilePosition, finalPosition);

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
