using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CameraRef), "res://editoricons/Component.svg", nameof(Resource))]
public class CameraRef : Component
{
    public Camera2D Camera { get; set; }
}
