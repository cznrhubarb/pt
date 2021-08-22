using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(TileLocation), "res://editoricons/Component.svg", nameof(Resource))]
public class TileLocation : Component
{
    public Vector3 TilePosition { get; set; }

    [Export]
    public int ZLayer { get; set; } = 5;
}
