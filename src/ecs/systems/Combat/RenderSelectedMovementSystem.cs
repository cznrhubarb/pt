using Ecs;
using Godot;
using System;

public class RenderSelectedMovementSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";

    public RenderSelectedMovementSystem()
    {
        AddRequiredComponent<TravelLocation>();
        AddRequiredComponent<Selected>(SelectedEntityKey);
        AddRequiredComponent<Movable>(SelectedEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var movingActor = SingleEntityFor(SelectedEntityKey);
        entity.Visible = movingActor?.GetComponent<Movable>().StartingLocation == null;
    }
}
