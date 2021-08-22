using Ecs;
using System.Collections.Generic;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(Movable), "res://editoricons/Component.svg", nameof(Resource))]
public class Movable : Component
{
    public int MaxMove { get; set; }

    public int MaxJump { get; set; }

    public int TravelSpeed { get; set; }

    public Dictionary<TerrainType, float> TerrainCostModifiers { get; set; } = new Dictionary<TerrainType, float>();

    public TileLocation StartingLocation { get; set; } = null;

    // TODO: This is duplicated across profileDetails
    public Affiliation Affiliation { get; set; }
}
