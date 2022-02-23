using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Directionality), "res://editoricons/Component.svg", nameof(Resource))]
public class Directionality : Component
{
    [Export]
    public Direction Direction { get; set; } = Direction.Down;
}
