using Ecs;
using Godot;
using System.Linq;

public class TravelToLocationSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string TravelLocationEntityKey = "travelLocation";

    public TravelToLocationSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TravelLocation>(TravelLocationEntityKey);
        AddRequiredComponent<TileLocation>(TravelLocationEntityKey);
        AddRequiredComponent<Selected>(SelectedEntityKey);
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
                var actorMovable = movingActor.GetComponent<Movable>();
                var actorLocation = movingActor.GetComponent<TileLocation>();
                actorMovable.StartingLocation = actorLocation.Clone() as TileLocation;
                actorLocation.TilePosition = reticleLocationComp.TilePosition;
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
}
