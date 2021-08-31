using Ecs;
using Godot;

public class CameraAnchoringSystem : Ecs.System
{
    private const string CameraKey = "camera";

    public CameraAnchoringSystem()
    {
        AddRequiredComponent<CameraAnchor>();
        AddRequiredComponent<CameraWrap>(CameraKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        Globals.cameraAnchorOffsets.Clear();

        base.UpdateAll(deltaTime);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var camera = SingleEntityFor(CameraKey).GetComponent<CameraWrap>().Camera;

        Globals.cameraAnchorOffsets[entity.GetComponent<CameraAnchor>().Name] = camera.Position - entity.Position;
    }
}
