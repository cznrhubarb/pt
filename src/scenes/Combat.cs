using Godot;
using System.Linq;
using System.Collections.Generic;

public class Combat : Ecs.Manager
{
    private List<TileMap> tilemaps = null;
    private Node2D target = null;
    private Camera2D camera = null;

    public override void _Ready()
    {
        base._Ready();

        tilemaps = new List<TileMap>();
        foreach (var child in this.GetChildren())
        {
            if (child is TileMap map)
            {
                tilemaps.Insert(0, map);
            }
        }

        target = FindNode("target") as Node2D;
        camera = FindNode("Camera2D") as Camera2D;
    }

    public override void _Input(InputEvent inputEvent)
    {
        base._Input(inputEvent);

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            var mousePos = mouseMotion.GlobalPosition + camera.Position;
            // TODO: Slight problem in not being able to select tiles that are behind other tiles
            // TODO: No multi-level map support (bridges, etc)
            for (var i = 0; i < tilemaps.Count; i++)
            {
                var tilemap = tilemaps[i];

                var tilePos = tilemap.WorldToMap(mousePos - tilemap.Position);
                if (tilemap.GetCell((int)tilePos.x, (int)tilePos.y) != TileMap.InvalidCell)
                {
                    var targetMap = tilemap;
                    while (i > 0 && tilemaps[i-1].GetCell((int)tilePos.x, (int)tilePos.y) != TileMap.InvalidCell)
                    {
                        targetMap = tilemaps[i - 1];
                        i--;
                    }
                    target.Position = targetMap.MapToWorld(tilePos) + new Vector2(0, 24) + targetMap.Position;
                    break;
                }
            }
        }
    }
}
