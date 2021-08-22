using Ecs;
using System.Collections.Generic;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(StatusBag), "res://editoricons/Component.svg", nameof(Resource))]
public class StatusBag : Component
{
    public Dictionary<string, StatusEffect> Statuses { get; set; } = new Dictionary<string, StatusEffect>();
}
