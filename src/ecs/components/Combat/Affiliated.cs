using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Affiliated), "res://editoricons/Component.svg", nameof(Resource))]
public class Affiliated : Component
{
    public Affiliation Affiliation { get; set; }
}
