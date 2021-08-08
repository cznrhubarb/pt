using Ecs;
using Godot;
using System;
using System.Linq;

public class SelectActionLocationSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string ActionLocationEntityKey = "actionLocation";
    private const string MapEntityKey = "map";
    private const string TargetedKey = "targeted";

    public SelectActionLocationSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TargetLocation>(ActionLocationEntityKey);
        AddRequiredComponent<TileLocation>(ActionLocationEntityKey);
        AddRequiredComponent<Selected>(SelectedEntityKey);
        AddRequiredComponent<FightStats>(SelectedEntityKey);
        AddRequiredComponent<TileLocation>(SelectedEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
        AddRequiredComponent<Targeted>(TargetedKey);
        AddRequiredComponent<FightStats>(TargetedKey);
        AddRequiredComponent<StatusBag>(TargetedKey);
        AddRequiredComponent<Health>(TargetedKey);
        AddRequiredComponent<TileLocation>(TargetedKey);
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
                if (manager.CurrentState is PlayerTargetingState ptState)
                {
                    ptState.SelectedSkill.CurrentTP--;
                    TargetUtils.PerformAction(manager, EntitiesFor(TargetedKey));
                    manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
                }
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
            if (manager.CurrentState is PlayerTargetingState ptState)
            {
                movingActor.GetComponent<TurnSpeed>().TimeToAct -= ptState.SelectedSkill.Speed;
            }

            manager.ApplyState(new PlayerMovementState(movingActor, SingleEntityFor(MapEntityKey)));
        }
    }
}
