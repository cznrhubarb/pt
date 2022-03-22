
public class CSEStartDialog : CutSceneEvent
{
    public string DialogName { get; set; } = "";

    public override void RunStep()
    {
        // TODO: Both this and the scene change are duplicated by the Cue system in Exploration/Manager
        var dialog = DialogicSharp.Start(DialogName, false);
        Manager.AddChild(dialog);
        dialog.Connect("dialogic_signal", this, nameof(DialogSignal));
        dialog.Connect("timeline_end", this, nameof(DialogFinished));
    }

    private void DialogFinished(string _timelineName)
    {
        OnComplete();
    }

    // TODO: Copied wholesale from Exploration. This is awful. I just want the screen transition to work :(
    private void DialogSignal(string parameter)
    {
        switch (parameter)
        {
            case "StartFight":
                Transition.To("res://src/scenes/Combat.tscn");
                break;
        }
    }
}
