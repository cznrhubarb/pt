using Ecs;
using Godot;

public class CSETurnActorAbsolute : CutSceneEvent
{
    public NodePath ActorPath { get; set; } = null;
    public Direction Direction { get; set; } = Direction.Up;

    public override void RunStep()
    {
        var actor = Manager.GetNode(ActorPath) as Entity;
        actor.GetComponent<Directionality>().Direction = Direction;
        OnComplete();
    }
}
