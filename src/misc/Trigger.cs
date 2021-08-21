using Godot;

public class Trigger : Node2D
{
    [Export]
    public CueType Cue { get; set; } = CueType.StartDialog;

    [Export]
    public string[] Params { get; set; } = new string[0];
}

public enum CueType
{
    StartDialog,
    ChangeScene
}