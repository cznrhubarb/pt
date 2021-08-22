using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(ProfileDetails), "res://editoricons/Component.svg", nameof(Resource))]
public class ProfileDetails : Component
{
    public string Name { get; set; }
    public int MonNumber { get; set; }
    public Affiliation Affiliation { get; set; }
    public int Level { get; set; } = 1;
}
