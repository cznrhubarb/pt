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

                // TODO: Maybe handle this a different way. If we have an "on end tween" trigger at some point, this won't work.
                if (path.Length > 1)
                {
                    MapUtils.BuildTweenForActor(manager, movingActor, path);
                }

                turnSpeed.TimeToAct = 20 + (path.Length - 1) * actorMovable.TravelSpeed;

                actorMovable.StartingLocation = actorLocation.Duplicate() as TileLocation;

                manager.AddComponentToEntity(manager.GetNewEntity(), new SetActionsDisplayStateEvent() { Displayed = true });
            }
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            var actorMovable = movingActor.GetComponent<Movable>();

            // TODO: Not sure when it happened, but we have a bug here now (Damn not having unit tests!)
            //  Going back sends the player back to the origin or something right now?
            if (actorMovable.StartingLocation != null)
            {
                if (movingActor.HasComponent<Tweening>())
                {
                    movingActor.GetComponent<Tweening>().TweenSequence.Kill();
                }
                turnSpeed.TimeToAct = 20;
                var actorLocation = movingActor.GetComponent<TileLocation>();
                actorLocation.TilePosition = actorMovable.StartingLocation.TilePosition;
                actorMovable.StartingLocation = null;
                manager.AddComponentToEntity(manager.GetNewEntity(), new SetActionsDisplayStateEvent() { Displayed = false });
            }
        }
    }
}
