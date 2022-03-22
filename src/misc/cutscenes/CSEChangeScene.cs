
public class CSEChangeScene : CutSceneEvent
{
    public string SceneName { get; set; } = "";

    public override void RunStep()
    {
        Transition.To($"res://src/scenes/{SceneName}.tscn");
        OnComplete();
    }
}
