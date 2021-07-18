using Ecs;
using Godot;
using System.Linq;

public class TravelToLocationSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string TravelLocationEntityKey = "travelLocation";
    private const string MapEntityKey = "map";

    public TravelToLocationSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TravelLocation>(TravelLocationEntityKey);
        AddRequiredComponent<TileLocation>(TravelLocationEntityKey);
        AddRequiredComponent<Selected>(SelectedEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var movingActor = SingleEntityFor(SelectedEntityKey);
        if (movingActor == null)
        {
            return;
        }

        var turnSpeed = movingActor.GetComponent<TurnSpeed>();
        if (Input.IsActionJustPressed("ui_accept"))
        {
            var potentialLocations = EntitiesFor(TravelLocationEntityKey)
                .Where(ent => ent.Visible)
                .Select(ent => ent.GetComponent<TileLocation>());

            var reticleLocationComp = entity.GetComponent<TileLocation>();
            var targetLocation = potentialLocations.FirstOrDefault(
                location =>
                 location.TilePosition == reticleLocationComp.TilePosition);

            if (targetLocation != null && movingActor.HasComponent<PlayerCharacter>())
            {
                var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
                var actorMovable = movingActor.GetComponent<Movable>();
                var actorLocation = movingActor.GetComponent<TileLocation>();
                var path = map.AStar.GetPath(actorMovable, actorLocation.TilePosition, reticleLocationComp.TilePosition);

                BuildTweenForActor(movingActor, map, path);

                turnSpeed.TimeToAct = 20 + (path.Length - 1) * actorMovable.TravelSpeed;

                actorMovable.StartingLocation = actorLocation.Clone() as TileLocation;

                manager.AddComponentToEntity(manager.GetNewEntity(), new SetActionsDisplayStateEvent() { Displayed = true });
            }
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            var actorMovable = movingActor.GetComponent<Movable>();

            if (actorMovable.StartingLocation != null)
            {
                if (movingActor.HasComponent<Tweening>())
                {
                    movingActor.GetComponent<Tweening>().TweenSequence.Kill();
                    manager.RemoveComponentFromEntity<Tweening>(movingActor);
                }
                turnSpeed.TimeToAct = 20;
                var actorLocation = movingActor.GetComponent<TileLocation>();
                actorLocation.TilePosition = actorMovable.StartingLocation.TilePosition;
                actorMovable.StartingLocation = null;
                manager.AddComponentToEntity(manager.GetNewEntity(), new SetActionsDisplayStateEvent() { Displayed = false });
            }
        }
    }

    private void BuildTweenForActor(Entity movingActor, Map map, Vector3[] path)
    {
        var tileLocation = movingActor.GetComponent<TileLocation>();
        var tweenSeq = new TweenSequence(manager.GetTree());
        for (var idx = 1; idx < path.Length; idx++)
        {
            // TODO: Slightly roundabout way of tweening our actors, primarily for the benefit of z sorting, and it still isn't perfect :(
            if (path[idx].z != path[idx - 1].z)
            {
                // Jump
                var ease = path[idx].z > path[idx - 1].z ? Tween.EaseType.Out : Tween.EaseType.In;
                tweenSeq.AppendInterval(0.1f);
                tweenSeq.AppendMethod(movingActor, "SetTilePositionXY", path[idx - 1], path[idx], 0.2f);
                tweenSeq.Join();
                tweenSeq.AppendMethod(movingActor, "SetTilePositionZ", path[idx - 1].z, path[idx].z, 0.2f)
                    .SetTransition(Tween.TransitionType.Back)
                    .SetEase(ease);
                tweenSeq.AppendInterval(0.1f);
            }
            else
            {
                // Walk
                tweenSeq.AppendMethod(movingActor, "SetTilePositionXY", path[idx - 1], path[idx], 0.3f);
            }
        }
        tweenSeq.Connect("finished", this, nameof(CleanUpTween), new Godot.Collections.Array() { movingActor });
        manager.AddComponentToEntity(movingActor, new Tweening() { TweenSequence = tweenSeq });
    }

    private void CleanUpTween(Entity entity)
    {
        manager.RemoveComponentFromEntity<Tweening>(entity);
    }

    private void UpdateTilePosition(Entity movingEntity, Vector3 position)
    {
        movingEntity.GetComponent<TileLocation>().TilePosition = position;
    }
}
