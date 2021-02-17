using Ecs;
using Godot;

public class CameraWrap : WrapComponent<Camera2D>
{
    public Camera2D Camera
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
