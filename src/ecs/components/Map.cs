using Ecs;
using Godot;
using System.Collections.Generic;

public class Map : Component
{
    public List<TileMap> TileMaps { get; private set; }

    public AStarEx AStar { get; private set; }

    public override void GrabReferences(Entity owner)
    {
        TileMaps = new List<TileMap>();
        foreach (var child in owner.GetChildren())
        {
            if (child is TileMap map)
            {
                TileMaps.Add(map);
            }
        }

        // Not the most ECS-esque thing, but it is what it is
        AStar = new AStarEx(TileMaps);
    }
}
