using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Terrain), "res://editoricons/Component.svg", nameof(Resource))]
public class Terrain : Component
{
    public TerrainType Type { get; set; }
}
