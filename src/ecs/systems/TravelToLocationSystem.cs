using Ecs;
using Godot;
using System.Linq;

public class TravelToLocationSystem : Ecs.System
{
    private const string TravelLocationEntityKey = "travelLocation";

    private bool selectThisFrame;
    private bool lastSelectThisFrame;

    public TravelToLocationSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TravelLocation>(TravelLocationEntityKey);
        AddRequiredComponent<TileLocation>(TravelLocationEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var potentialLocations = EntitiesFor(TravelLocationEntityKey).Select(ent => ent.GetComponent<TileLocation>());
        var movingActor = entity.GetComponent<Reticle>().CurrentTarget;

        if (selectThisFrame && !lastSelectThisFrame && movingActor != null)
        {
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
            }
        }

        lastSelectThisFrame = selectThisFrame;
        selectThisFrame = false;
    }

    public override void _Input(InputEvent inputEvent)
    {
        base._Input(inputEvent);

        if (inputEvent is InputEventMouseButton mouseButton && (ButtonList)mouseButton.ButtonIndex == ButtonList.Left && mouseButton.Pressed)
        {
            selectThisFrame = true;
        }
    }
}
