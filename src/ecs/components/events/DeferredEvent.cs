using Ecs;
using System;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(DeferredEvent), "res://editoricons/Component.svg", nameof(Resource))]
public class DeferredEvent : Component
{
    public Action Callback { get; set; }
    public float Delay { get; set; } = 0;
}
