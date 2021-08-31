using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CameraAnchor), "res://editoricons/Component.svg", nameof(Resource))]
public class CameraAnchor : Component
{
    [Export]
    public string Name { get; set; } = "";
}
