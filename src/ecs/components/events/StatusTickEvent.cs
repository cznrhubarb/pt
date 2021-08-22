using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(StatusTickEvent), "res://editoricons/Component.svg", nameof(Resource))]
public class StatusTickEvent : Component
{
    public Entity TickingEntity { get; set; }
}
