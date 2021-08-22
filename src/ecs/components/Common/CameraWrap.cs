using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CameraWrap), "res://editoricons/Component.svg", nameof(Resource))]
public class CameraWrap : WrapComponent<Camera2D>
{
    public Camera2D Camera
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
