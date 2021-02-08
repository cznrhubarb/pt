using Ecs;
using Godot;

public class SelectActorSystem : Ecs.System
{
    private const string SelectableEntityKey = "selectable";

    private bool selectThisFrame;

    public SelectActorSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Selectable>(SelectableEntityKey);
        AddRequiredComponent<TileLocation>(SelectableEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var selectableEntities = EntitiesFor(SelectableEntityKey);

        if (selectThisFrame)
        {
            var tileLocationComp = entity.GetComponent<TileLocation>();
            var reticleComp = entity.GetComponent<Reticle>();

            if (reticleComp.CurrentTarget != null)
            {
                reticleComp.CurrentTarget.GetComponent<Selectable>().Selected = false;
                reticleComp.CurrentTarget = null;
            }

            foreach (var target in selectableEntities)
            {
                var targetLocationComp = target.GetComponent<TileLocation>();
                if (targetLocationComp.TilePosition == tileLocationComp.TilePosition &&
                    targetLocationComp.Height == tileLocationComp.Height)
                {
                    var selectableComp = target.GetComponent<Selectable>();
                    selectableComp.Selected = true;

                    reticleComp.CurrentTarget = target;
                }
            }

            GD.Print(reticleComp.CurrentTarget?.Name);
            selectThisFrame = false;
        }
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
