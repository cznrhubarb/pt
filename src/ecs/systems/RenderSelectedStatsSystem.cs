using Ecs;
using Godot;
using System;

public class RenderSelectedStatsSystem : Ecs.System
{
    public RenderSelectedStatsSystem()
    {
        AddRequiredComponent<Selectable>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var selectableComp = entity.GetComponent<Selectable>();

        if (selectableComp.Selected)
        {
            entity.GetComponentOrNull<Speed>();
        }
    }
}
