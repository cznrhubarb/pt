using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CSEStartDialog), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
public class CSEStartDialog : CutSceneEvent
{
    [Export]
    public string DialogName { get; set; } = "";

    public override void RunStep()
    {
        var dialog = DialogicSharp.Start(DialogName, false);
        Manager.AddChild(dialog);
        dialog.Connect("timeline_end", this, nameof(DialogFinished));
    }

    private void DialogFinished(string _timelineName)
    {
        OnComplete();
    }
}
