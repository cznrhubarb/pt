using Ecs;
using Godot;

public class Tweening : Component
{
    public TweenSequence TweenSequence { get; set; }
    public bool DeleteEntityOnComplete { get; set; } = false;
}