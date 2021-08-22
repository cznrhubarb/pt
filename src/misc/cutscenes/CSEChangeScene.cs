﻿using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CSEChangeScene), "res://editoricons/CutSceneEvent.svg", nameof(Resource))]
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