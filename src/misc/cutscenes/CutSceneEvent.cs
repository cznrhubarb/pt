using Ecs;
using Godot;
using System;

public abstract class CutSceneEvent : Resource
{
    public Manager Manager { get; set; }
    public Action OnComplete { get; set; }

    public abstract void RunStep();
}
