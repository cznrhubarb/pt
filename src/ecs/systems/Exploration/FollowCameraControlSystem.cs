using Ecs;
using Godot;

public class FollowCameraControlSystem : Ecs.System
{
    private const string CameraKey = "camera";
    private const float FollowSpeed = 5;

    public FollowCameraControlSystem()
    {
        AddRequiredComponent<Selected>();
        AddRequiredComponent<CameraWrap>(CameraKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var camera = SingleEntityFor(CameraKey).GetComponent<CameraWrap>().Camera;
        
        camera.Position = camera.Position.LinearInterpolate(entity.Position, FollowSpeed * deltaTime);
    }
}
