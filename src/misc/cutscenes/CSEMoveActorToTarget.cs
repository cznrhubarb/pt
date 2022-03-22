using Ecs;
using Godot;

public class CSEMoveActorToTarget : CSEMoveActorAbsolute
{
    public NodePath TargetPath { get; set; } = null;

    public override void RunStep()
    {
        var target = Manager.GetNode(TargetPath) as Entity;
        FinalPosition = target.GetComponent<TileLocation>().TilePosition;

        base.RunStep();
    }
}
