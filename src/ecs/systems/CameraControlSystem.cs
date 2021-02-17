using Ecs;
using Godot;

public class CameraControlSystem : Ecs.System
{
    private const float CameraMoveSpeed = 400;

    public CameraControlSystem()
    {
        AddRequiredComponent<CameraWrap>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var camera = entity.GetComponent<CameraWrap>().Camera;

        if (Input.IsActionPressed("camera_right"))
        {
            camera.MoveLocalX(CameraMoveSpeed * deltaTime);
        }
        else if (Input.IsActionPressed("camera_left"))
        {
            camera.MoveLocalX(-CameraMoveSpeed * deltaTime);
        }

        if (Input.IsActionPressed("camera_down"))
        {
            camera.MoveLocalY(CameraMoveSpeed * deltaTime);
        }
        else if (Input.IsActionPressed("camera_up"))
        {
            camera.MoveLocalY(-CameraMoveSpeed * deltaTime);
        }
    }
}
