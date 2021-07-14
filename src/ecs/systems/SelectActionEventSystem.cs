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

        if (actionEvent.SelectedMove != null)
        {
            manager.ApplyState(new PlayerTargetingState() { Acting = acting, SelectedMove = actionEvent.SelectedMove, Map = SingleEntityFor(MapEntityKey) });
        }
        else
        {
            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
        }
        manager.DeleteEntity(entity.Id);
    }
}
