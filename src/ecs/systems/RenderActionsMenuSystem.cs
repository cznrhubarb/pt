using Ecs;
using System.Linq;

public class RenderActionsMenuSystem : Ecs.System
{
    private const string ActionMenuEntityKey = "actionMenu";

    public RenderActionsMenuSystem()
    {
        AddRequiredComponent<MoveSet>();
        AddRequiredComponent<ActionMenu>(ActionMenuEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var movableComp = entity.GetComponentOrNull<Movable>();
        var menuEntity = SingleEntityFor(ActionMenuEntityKey);

        var shouldBeVisible = movableComp == null || movableComp.StartingLocation != null;
        var menuComp = menuEntity.GetComponent<ActionMenu>();
        if (!menuComp.Visible && shouldBeVisible)
        {
            // Set the buttons
            menuComp.Actions = entity.GetComponent<MoveSet>().Moves.Select(m => m.Name).ToList();
        }
        menuComp.Visible = shouldBeVisible;
    }
}
