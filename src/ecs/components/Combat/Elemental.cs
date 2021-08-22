using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Elemental), "res://editoricons/Component.svg", nameof(Resource))]
public class Elemental : Component
{
    public Element Element { get; set; }
}
