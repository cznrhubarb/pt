using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Pulse), "res://editoricons/Component.svg", nameof(Resource))]
public class Pulse : Component
{
    public float squishSpeed { get; set; }

    public float squishAmountX { get; set; }

    public float squishAmountY { get; set; }

    public float squishAccumulator { get; set; } = 0;
}
