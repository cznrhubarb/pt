using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

[RegisteredType(nameof(CutScene), "res://editoricons/CutScene.svg", nameof(Resource))]
public class CutScene : Resource
{
    public List<CutSceneEvent> Events { get; set; } = new List<CutSceneEvent>();

    public CutScene() { }
}
