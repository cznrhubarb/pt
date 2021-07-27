using Ecs;
using Godot;

public class RenderStatusEffectsSystem : Ecs.System
{
    public RenderStatusEffectsSystem()
    {
        AddRequiredComponent<StatusBag>();
        AddRequiredComponent<TileLocation>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        // Display bubble on rotating timer


        // Display on profile card if it is assigned to this entity
        manager.PerformHudAction("UpdateStatusEffects", entity);
    }
}
