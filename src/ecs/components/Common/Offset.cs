using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Offset), "res://editoricons/Component.svg", nameof(Resource))]
public class Offset : Component
{
    // Probably need some way of turning a 3d vector into a 2d vector for tweening. Should be as simple as going 3d to 2d on both endpoints of the vector though.
    public Vector2 Amount { get; set; } = Vector2.Zero;

    public void SetAmount(Vector2 newAmount) => Amount = newAmount;
}
