using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(TurnSpeed), "res://editoricons/Component.svg", nameof(Resource))]
public class TurnSpeed : Component
{
    public int TimeToAct { get; set; }
}
