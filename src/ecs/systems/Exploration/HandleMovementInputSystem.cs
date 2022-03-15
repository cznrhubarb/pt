using Ecs;
using Godot;
using System.Collections.Generic;

public class HandleMovementInputSystem : Ecs.System
{
    private const string MapKey = "map";
    private const string WalkOnTriggerKey = "walkOnTrigger";

    private const int MaxJumpHeight = 2;
    private HashSet<TerrainType> impassableTerrain;

    public HandleMovementInputSystem()
    {
        AddRequiredComponent<Selected>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Directionality>();
        AddRequiredComponent<Map>(MapKey);
        AddRequiredComponent<TileLocation>(WalkOnTriggerKey);
        AddRequiredComponent<WalkOnTrigger>(WalkOnTriggerKey);

        impassableTerrain = new HashSet<TerrainType>() { TerrainType.DeepWater, TerrainType.Water };
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        if (entity.HasComponent<Tweening>())
        {
            return;
        }

        if (Input.IsActionPressed("walk_right"))
        {
            BuildTween(entity, Vector3.Right);
            entity.GetComponent<Directionality>().Direction = Direction.Right;
        }
        else if (Input.IsActionPressed("walk_left"))
        {
            BuildTween(entity, Vector3.Left);
            entity.GetComponent<Directionality>().Direction = Direction.Left;
        }
        else if (Input.IsActionPressed("walk_down"))
        {
            // Inverted intentionally due to camera coordinate system
            BuildTween(entity, Vector3.Up);
            entity.GetComponent<Directionality>().Direction = Direction.Down;
        }
        else if (Input.IsActionPressed("walk_up"))
        {
            // Inverted intentionally due to camera coordinate system
            BuildTween(entity, Vector3.Down);
            entity.GetComponent<Directionality>().Direction = Direction.Up;
        }
    }

    private void BuildTween(Entity movingActor, Vector3 direction)
    {
        var oldPos = movingActor.GetComponent<TileLocation>().TilePosition;

        var map = SingleEntityFor(MapKey).GetComponent<Map>();
        var newPosMaybe = map.AStar.GetBestTileMatch(oldPos + direction, MaxJumpHeight, impassableTerrain);
        if (newPosMaybe == null)
        {
            return;
        }

        var newPos = newPosMaybe.Value;
        var tweenSeq = new TweenSequence(manager.GetTree());
        if (newPos.z != oldPos.z)
        {
            // Jump
            var ease = newPos.z > oldPos.z ? Tween.EaseType.Out : Tween.EaseType.In;
            tweenSeq.AppendInterval(0.1f);
            tweenSeq.AppendMethod(movingActor, "SetTilePositionXY", oldPos, newPos, 0.2f);
            tweenSeq.Join();
            tweenSeq.AppendMethod(movingActor, "SetTilePositionZ", oldPos.z, newPos.z, 0.2f)
                .SetTransition(Tween.TransitionType.Back)
                .SetEase(ease);
            tweenSeq.AppendInterval(0.1f);
        }
        else
        {
            // Walk
            tweenSeq.AppendMethod(movingActor, "SetTilePositionXY", oldPos, newPos, 0.2f);
        }
        manager.AddComponentToEntity(movingActor, new Tweening() { TweenSequence = tweenSeq });

        var potentialTriggers = EntitiesFor(WalkOnTriggerKey);
        foreach (var trigger in potentialTriggers)
        {
            if (trigger.GetComponent<TileLocation>().TilePosition == newPos)
            {
                var triggerComp = trigger.GetComponent<WalkOnTrigger>();
                // TODO/HACK: Strong coupling. Fixed by moving TriggerCue out to Manager
                //  but that comes with the extra cost of having to move all the dialog
                //  stuff currently in exploration only to combat also :(
                if (manager is Exploration ex)
                {
                    ex.TriggerCue(triggerComp.Cue, triggerComp.CueParam);
                }
                break;
            }
        }
    }
}
