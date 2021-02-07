using Godot;

public class TileLocation : Ecs.Component
{
    public Vector2 TilePosition { get; set; }
   
    public int Height { get; set; }

    public Map MapRef { get; set; }

    public int ZLayer { get; set; }
}
