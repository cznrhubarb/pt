using Ecs;
using System.Collections.Generic;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(SkillSet), "res://editoricons/Component.svg", nameof(Resource))]
public class SkillSet : Component
{
    public List<Skill> Skills { get; set; }
}
