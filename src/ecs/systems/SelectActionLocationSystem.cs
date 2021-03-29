using Ecs;
using Godot;
using System.Linq;

public class SelectActionLocationSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string ActionLocationEntityKey = "actionLocation";
    private const string MapEntityKey = "map";

    public SelectActionLocationSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TravelLocation>(ActionLocationEntityKey);
        AddRequiredComponent<TileLocation>(ActionLocationEntityKey);
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
            var potentialLocations = EntitiesFor(ActionLocationEntityKey)
                .Where(ent => ent.Visible)
                .Select(ent => ent.GetComponent<TileLocation>());

            var reticleLocationComp = entity.GetComponent<TileLocation>();
            var targetLocation = potentialLocations.FirstOrDefault(
                location =>
                 location.TilePosition == reticleLocationComp.TilePosition);

            if (targetLocation != null && movingActor.HasComponent<PlayerCharacter>())
            {
                // TODO: Perform the action
                manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
            }

            // Little hacky, but we don't want to clear this until they have committed to a turn
            var movableComp = movingActor.GetComponentOrNull<Movable>();
            if (movableComp != null)
            {
                movableComp.StartingLocation = null;
            }
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            // TODO: Maybe also need to reset that the movement has not been reset yet
            manager.ApplyState(new PlayerMovementState() { Acting = SingleEntityFor(SelectedEntityKey), Map = SingleEntityFor(MapEntityKey) });
        }
    }
}
