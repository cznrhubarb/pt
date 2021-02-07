using Ecs;
using Godot;

public class SelectActorSystem : Ecs.DyadicSystem
{
    private bool selectThisFrame;

    public SelectActorSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredSecondaryComponent<Selectable>();
        AddRequiredSecondaryComponent<TileLocation>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        if (selectThisFrame)
        {
            var tileLocationComp = entity.GetComponent<TileLocation>();
            var reticleComp = entity.GetComponent<Reticle>();

            if (reticleComp.CurrentTarget != null)
            {
                reticleComp.CurrentTarget.GetComponent<Selectable>().Selected = false;
                reticleComp.CurrentTarget = null;
            }

            foreach (var target in SecondaryEntities)
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
