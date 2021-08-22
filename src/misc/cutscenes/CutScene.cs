using Ecs;
using Godot;
using System;

public class CutScene : Resource
{
    [Export]
    public CutSceneEvent[] Events { get; set; } = new CutSceneEvent[0];

    public CutScene() { }
}
