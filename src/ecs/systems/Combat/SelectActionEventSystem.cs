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
                movableComp.StartingLocation = null;
            }
            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
        }
        manager.DeleteEntity(entity.Id);
    }
}
