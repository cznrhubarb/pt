using Ecs;
using Godot;
using System;

public abstract class CutSceneEvent : Resource
{
    // MIKE_TODO: Remove all the export tags from the events when migration is complete

    public Manager Manager { get; set; }
    public Action OnComplete { get; set; }

    public abstract void RunStep();
}
