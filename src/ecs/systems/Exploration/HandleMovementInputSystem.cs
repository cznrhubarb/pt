using Ecs;
using Godot;

public class HandleMovementInputSystem : Ecs.System
{
    private const string MapKey = "map";

    private const int MaxJumpHeight = 2;

    public HandleMovementInputSystem()
    {
        AddRequiredComponent<Selected>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Map>(MapKey);
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
        }
        else if (Input.IsActionPressed("walk_left"))
        {
            BuildTween(entity, Vector3.Left);
        }
        else if (Input.IsActionPressed("walk_down"))
        {
            // Inverted intentionally
            BuildTween(entity, Vector3.Up);
        }
        else if (Input.IsActionPressed("walk_up"))
        {
            // Inverted intentionally
            BuildTween(entity, Vector3.Down);
        }
    }

    private void BuildTween(Entity movingActor, Vector3 direction)
    {
        var oldPos = movingActor.GetComponent<TileLocation>().TilePosition;

        var map = SingleEntityFor(MapKey).GetComponent<Map>();
        var newPosMaybe = map.AStar.GetBestTileMatch(oldPos + direction, MaxJumpHeight);
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
    }
}
