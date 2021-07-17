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

                BuildTweenForActor(movingActor, map, map.AStar.GetPath(actorMovable, actorLocation.TilePosition, reticleLocationComp.TilePosition));

                actorMovable.StartingLocation = actorLocation.Clone() as TileLocation;

                manager.AddComponentToEntity(manager.GetNewEntity(), new SetActionsDisplayStateEvent() { Displayed = true });
            }
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            var actorMovable = movingActor.GetComponent<Movable>();

            if (actorMovable.StartingLocation != null)
            {
                var actorLocation = movingActor.GetComponent<TileLocation>();
                actorLocation.TilePosition = actorMovable.StartingLocation.TilePosition;
                actorMovable.StartingLocation = null;
                manager.AddComponentToEntity(manager.GetNewEntity(), new SetActionsDisplayStateEvent() { Displayed = false });
            }
        }
    }

    private void BuildTweenForActor(Entity movingActor, Map map, Vector3[] path)
    {
        var tweenSeq = new TweenSequence(manager.GetTree());
        for (var idx = 1; idx < path.Length; idx++)
        {
            var screenDest = map.IsoMap.MapToWorld(path[idx]) + new Vector2(0, 24);
            if (path[idx].z != path[idx - 1].z)
            {
                // Jump
                var ease = path[idx].z > path[idx - 1].z ? Tween.EaseType.Out : Tween.EaseType.In;
                tweenSeq.AppendInterval(0.1f);
                tweenSeq.Append(movingActor, "position:x", screenDest.x, 0.2f);
                tweenSeq.Join();
                tweenSeq.Append(movingActor, "position:y", screenDest.y, 0.2f)
                    .SetTransition(Tween.TransitionType.Back)
                    .SetEase(ease);
                tweenSeq.AppendInterval(0.1f);
            }
            else
            {
                // Walk
                tweenSeq.Append(movingActor, "position", screenDest, 0.3f);
            }

            // TODO: Fix this. This is intended to fix z-sorting while transitioning, but it doesn't /quite/ do it.
            //  Potentially need to do this before or after the animation tweens depending on which way we're heading
            //  in the Z-sort. But that could also potentially have problems.
            tweenSeq.AppendCallback(this, nameof(UpdateTilePosition), new object[] { movingActor, path[idx] });
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
