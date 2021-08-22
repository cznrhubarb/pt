using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Tweening), "res://editoricons/Component.svg", nameof(Resource))]
public class Tweening : Component
{
    public TweenSequence TweenSequence { get; set; }
    public bool DeleteEntityOnComplete { get; set; } = false;
}
