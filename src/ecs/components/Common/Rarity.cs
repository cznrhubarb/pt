using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Rarity), "res://editoricons/Component.svg", nameof(Resource))]
public class Rarity : Component
{
    [Export]
    public RarityValue Value { get; set; } = RarityValue.Common;
}