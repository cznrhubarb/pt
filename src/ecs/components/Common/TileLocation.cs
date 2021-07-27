using Ecs;
using Godot;

public class TileLocation : Component
{
    public Vector3 TilePosition { get; set; }

    public int ZLayer { get; set; }
}
