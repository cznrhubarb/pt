using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(SetActionsDisplayStateEvent), "res://editoricons/Component.svg", nameof(Resource))]
public class SetActionsDisplayStateEvent : Component
{
    public bool Displayed { get; set; }
}
