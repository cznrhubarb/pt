using Ecs;
using Godot;

public class CSEChangeScene : CutSceneEvent
{
    [Export]
    public string SceneName { get; set; } = "";

    public override void RunStep()
    {
        Manager.GetTree().ChangeScene($"res://src/scenes/{SceneName}.tscn");
        OnComplete();
    }
}
