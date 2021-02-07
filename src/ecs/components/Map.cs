using Ecs;
using Godot;
using System.Collections.Generic;

public class Map : Component
{
    public List<TileMap> TileMaps { get; private set; }

    public override void GrabReferences(Entity owner)
    {
        TileMaps = new List<TileMap>();
        foreach (var child in owner.GetChildren())
        {
            if (child is TileMap map)
            {
                TileMaps.Insert(0, map);
            }
        }
    }
}
