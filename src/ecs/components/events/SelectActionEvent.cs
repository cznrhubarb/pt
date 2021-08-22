using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(SelectActionEvent), "res://editoricons/Component.svg", nameof(Resource))]
public class SelectActionEvent : Component
{
    public Skill SelectedSkill { get; set; }
}
