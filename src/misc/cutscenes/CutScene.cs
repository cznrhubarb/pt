using Godot;
using System.Collections.Generic;

public class CutScene : Resource
{
    public List<CutSceneEvent> Events { get; set; } = new List<CutSceneEvent>();

    public CutScene() { }
}
