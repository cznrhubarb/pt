using Ecs;
using Godot;

public class UpdateGooglyEyeSystem : Ecs.System
{
    private Vector2 lastWindowPosition;

    public UpdateGooglyEyeSystem()
    {
        AddRequiredComponent<GooglyEyes>();
        lastWindowPosition = OS.WindowPosition;
    }

    public override void UpdateAll(float deltaTime)
    {
        base.UpdateAll(deltaTime);
        lastWindowPosition = OS.WindowPosition;
    }

    // This system is responsible for creating eyes if an entity has the correct component,
    //  applying physics forces to the eyes when appropriate, and constraining each iris
    //  to their ball. It does not update the position or turn force into velocity, as that is
    //  handled automatically by Godot.
    protected override void Update(Entity entity, float deltaTime)
    {
        var eyeComp = entity.GetComponent<GooglyEyes>();
        while (eyeComp.Eyes.Count < eyeComp.EyeOffsets.Count)
        {
            // Add a new eye
            var eye = new Eye() { Ball = new Sprite(), Iris = new Sprite(), IrisBody = new RigidBody2D(), Radius = eyeComp.EyeSizes[eyeComp.Eyes.Count] };
            eyeComp.Eyes.Add(eye);
        }

        // Constrain position (and apply counter force when constraining a large amount?)
        foreach (var eye in eyeComp.Eyes)
        {
            var delta = eye.Ball.Position - eye.Iris.Position;
            if (delta.Length() > eye.Radius)
            {
                eye.Iris.Position = eye.Ball.Position - delta * eye.Radius;
            }
        }

        // Apply force when the parent node is moving (position not same as last)
        if (eyeComp.LastParentPosition != null)
        {
            var delta = entity.Position - eyeComp.LastParentPosition.Value * deltaTime;
            foreach (var eye in eyeComp.Eyes)
            {
                eye.IrisBody.AddForce(eye.IrisBody.GlobalPosition, -delta);
            }
        }
        eyeComp.LastParentPosition = entity.Position;

        // Apply force when the window moves
        if (OS.WindowPosition != lastWindowPosition)
        {
            var windowDelta = OS.WindowPosition - lastWindowPosition * deltaTime;
            foreach (var eye in eyeComp.Eyes)
            {
                eye.IrisBody.AddForce(eye.IrisBody.GlobalPosition, -windowDelta);
            }
        }
    }
}
