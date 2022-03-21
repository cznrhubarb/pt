using Ecs;
using System.Collections.Generic;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(Targeted), "res://editoricons/Component.svg", nameof(Resource))]
public class Targeted : Component
{
    public float HitChance { get; set; }
    public float CritChance { get; set; } = 0;
    public Dictionary<string, object> TargetEffects { get; set; } = new Dictionary<string, object>();
}
