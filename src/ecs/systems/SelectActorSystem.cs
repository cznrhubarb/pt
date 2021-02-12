using Ecs;
using Godot;

public class SelectActorSystem : Ecs.System
{
    private const string SelectableEntityKey = "selectable";

    private bool selectThisFrame;
    private bool lastSelectThisFrame;

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

        if (selectThisFrame && !lastSelectThisFrame)
        {
            var tileLocationComp = entity.GetComponent<TileLocation>();
            var reticleComp = entity.GetComponent<Reticle>();

            if (reticleComp.CurrentTarget != null)
            {
                manager.RemoveComponentFromEntity<Selected>(reticleComp.CurrentTarget);
                reticleComp.CurrentTarget = null;
            }

            foreach (var target in selectableEntities)
            {
                var targetLocationComp = target.GetComponent<TileLocation>();
                if (targetLocationComp.TilePosition == tileLocationComp.TilePosition)
                {
                    manager.AddComponentToEntity(target, new Selected());
                    reticleComp.CurrentTarget = target;
                }
            }

            GD.Print(reticleComp.CurrentTarget?.Name);
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
