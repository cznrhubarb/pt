using Ecs;
using System.Collections.Generic;

public class Movable : Component
{
    public int MaxMove { get; set; }

    public int MaxJump { get; set; }

    public Dictionary<TerrainType, float> TerrainCostModifiers { get; set; } = new Dictionary<TerrainType, float>();

    public TileLocation StartingLocation { get; set; } = null;

    public Affiliation Affiliation { get; set; }
}
