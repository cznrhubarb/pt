using Ecs;
using Godot;

public class HandleInteractionInputSystem : Ecs.System
{
    public HandleInteractionInputSystem()
    {
        AddRequiredComponent<Selected>();
        AddRequiredComponent<TileLocation>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        if (entity.HasComponent<Tweening>())
        {
            return;
        }

        if (Input.IsActionPressed("ui_select"))
        {
            var dialog = DialogicSharp.Start("Greeting", false);
            manager.AddChild(dialog);
        }
    }
}
