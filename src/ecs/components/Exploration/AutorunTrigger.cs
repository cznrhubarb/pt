using Ecs;
using System;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(AutorunTrigger), "res://editoricons/Component.svg", nameof(Resource))]
public class AutorunTrigger : Component
{
    public Action Action { get; set; }
    [Export]
    public CueType Cue { get; set; } = CueType.StartDialog;
    [Export]
    public string CueParam { get; set; } = "";
}
