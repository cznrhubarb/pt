using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Health), "res://editoricons/Component.svg", nameof(Resource))]
public class Health : Component
{
    public int Current { get; set; }
    public int Max { get; set; }
}
