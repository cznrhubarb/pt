using Ecs;
using Godot;
using System.Collections.Generic;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Map), "res://editoricons/Component.svg", nameof(Resource))]
public class Map : Component
{
    public IsoMap IsoMap { get; private set; }

    public AStarEx AStar { get; private set; }

    public Map(List<Entity> tiles)
    {
        // Not the most ECS-esque thing, but it is what it is
        IsoMap = new IsoMap(tiles);
        AStar = new AStarEx(IsoMap);
    }
}
