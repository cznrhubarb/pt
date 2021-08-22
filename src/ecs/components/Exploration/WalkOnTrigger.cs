using Ecs;
using System;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(WalkOnTrigger), "res://editoricons/Component.svg", nameof(Resource))]
public class WalkOnTrigger : Component
{
    public Action Action { get; set; }
    [Export]
    public CueType Cue { get; set; } = CueType.StartDialog;
    [Export]
    public string CueParam { get; set; } = "";
}
