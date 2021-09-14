using Ecs;
using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

[RegisteredType(nameof(GooglyEyes), "res://editoricons/Component.svg", nameof(Resource))]
public class GooglyEyes : Component
{
    [Export]
    public List<Vector3> EyeOffsets { get; set; } = new List<Vector3>();

    [Export]
    public List<float> EyeSizes { get; set; } = new List<float>();

    public Vector2? LastParentPosition { get; set; } = null;

    public List<Eye> Eyes { get; set; } = new List<Eye>();
}

public class Eye : Node2D
{
    public Sprite Ball { get; set; }
    public Sprite Iris { get; set; }
    public float Radius { get; set; }
    public RigidBody2D IrisBody { get; set; }
}