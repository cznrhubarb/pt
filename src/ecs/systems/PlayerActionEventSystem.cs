using Ecs;
using Godot;

public class PlayerActionEventSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string MapEntityKey = "map";

    public PlayerActionEventSystem()
    {
        AddRequiredComponent<ActionMenu>();
        AddRequiredComponent<Selected>(SelectedEntityKey);
        AddRequiredComponent<MoveSet>(SelectedEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime) 
    {
        var menuComp = entity.GetComponent<ActionMenu>();
        // TODO: Not an actual event. Should it be though? If it was, we could include targeting information...
        if (menuComp.SelectedMenuAction != null)
        {
            var acting = SingleEntityFor(SelectedEntityKey);
            var selectedMove = acting.GetComponent<MoveSet>().Moves.Find(move => move.Name == menuComp.SelectedMenuAction);
            manager.ApplyState(new PlayerTargetingState() { Acting = acting, SelectedMove = selectedMove, Map = SingleEntityFor(MapEntityKey)});
            menuComp.SelectedMenuAction = null;
            menuComp.Visible = false;
        }
    }
}
