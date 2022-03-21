using Ecs;
using System.Collections.Generic;
using MonoCustomResourceRegistry;
using Godot;

[RegisteredType(nameof(Movable), "res://editoricons/Component.svg", nameof(Resource))]
public class Movable : Component
{
    [Export]
    public int MaxMove { get; set; }

    [Export]
    public int MaxJump { get; set; }

    [Export]
    public int TravelSpeed { get; set; }

    [Export]
    public Dictionary<TerrainType, float> TerrainCostModifiers { get; set; } = new Dictionary<TerrainType, float>();

    public TileLocation StartingLocation { get; set; } = null;

    public Movable Clone()
    {
        return new Movable()
        {
            MaxMove = MaxMove,
            MaxJump = MaxJump,
            TravelSpeed = TravelSpeed,
            // TODO: Should be deep copy
            TerrainCostModifiers = TerrainCostModifiers
        };
    }
}
