using Ecs;

public class SelectActionEventSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string MapEntityKey = "map";

    public SelectActionEventSystem()
    {
        AddRequiredComponent<SelectActionEvent>();
        AddRequiredComponent<Selected>(SelectedEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var actionEvent = entity.GetComponent<SelectActionEvent>();
        var acting = SingleEntityFor(SelectedEntityKey);

        // If the current state is PlayerTargetingState already, we need to undo the time delay we've incremented already
        if (manager.CurrentState is PlayerTargetingState pts)
        {
            var turnSpeed = acting.GetComponent<TurnSpeed>();
            turnSpeed.TimeToAct -= pts.SelectedSkill.Speed;
        }

        if (actionEvent.SelectedSkill != null)
        {
            manager.ApplyState(new PlayerTargetingState(acting, SingleEntityFor(MapEntityKey), actionEvent.SelectedSkill));
        }
        else
        {
            // Little hacky, but we don't want to clear this until they have committed to a turn
            //  Also duplicated between here and SelectActionLocationSystem
            var movableComp = acting.GetComponentOrNull<Movable>();
            if (movableComp != null)
            {
                if (movableComp.StartingLocation != acting.GetComponent<TileLocation>()
                    && acting.GetComponent<StatusBag>().Statuses.ContainsKey("Capturing"))
                {
                    // We moved, so break the capture.
                    acting.GetComponent<StatusBag>().Statuses.Remove("Capturing");
                    var captive = manager.GetEntitiesWithComponent<Captured>()[0];
                    captive.GetComponent<StatusBag>().Statuses.Remove("Captured");
                    manager.RemoveComponentFromEntity<Captured>(captive);
                }

                movableComp.StartingLocation = null;
            }

            // TODO: Would be better if we could apply these along the way instead of all at once
            //  that way if we are displaying it, it updates correctly.
            var statuses = acting.GetComponent<StatusBag>().Statuses;
            if (statuses.ContainsKey("Haste"))
            {
                acting.GetComponent<TurnSpeed>().TimeToAct /= 2;
            }
            else if (statuses.ContainsKey("Slow"))
            {
                acting.GetComponent<TurnSpeed>().TimeToAct *= 2;
            }
            manager.AddComponentToEntity(manager.GetNewEntity(), new StatusTickEvent() { TickingEntity = acting });
            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
        }
        manager.DeleteEntity(entity.Id);
    }
}
