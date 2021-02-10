using Ecs;

public class RenderSelectedMovementSystem : Ecs.System
{
    public RenderSelectedMovementSystem()
    {
        AddRequiredComponent<Selectable>();
        AddRequiredComponent<MoveStats>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var selectableComp = entity.GetComponent<Selectable>();

        if (selectableComp.Selected)
        {
            entity.GetComponentOrNull<MoveStats>();
        }
    }
}
