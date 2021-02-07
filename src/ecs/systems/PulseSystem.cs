using Ecs;
using Godot;
using System;

public class PulseSystem : Ecs.System
{
    public PulseSystem()
    {
        AddRequiredComponent<Pulse>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var pulseComp = entity.GetComponent<Pulse>();
        pulseComp.squishAccumulator += deltaTime * pulseComp.squishSpeed;
        float squishX = 1 + ((float)Math.Sin(pulseComp.squishAccumulator) * pulseComp.squishAmountX);
        float squishY = 1 + ((float)Math.Sin(pulseComp.squishAccumulator) * pulseComp.squishAmountY);

        entity.Scale = new Vector2(squishX, squishY);
    }
}
