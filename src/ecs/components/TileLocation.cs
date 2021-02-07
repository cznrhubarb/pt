using Ecs;
using Godot;

public class TileLocation : Component
{
    public Vector2 TilePosition { get; set; }
   
    public int Height { get; set; }

    public int ZLayer { get; set; }
}
