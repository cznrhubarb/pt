using Ecs;
using System.Collections.Generic;

public class MoveSet : Component
{
    public List<Move> Moves { get; set; }
}

// Subclass Godot.Object so that Move objects are marshallable for signals
public class Move : Godot.Object
{
    public string Name { get; set; }
    public Element Element { get; set; }
    public int CurrentTP { get; set; }
    public int MaxTP { get; set; }
    public int MinRange { get; set; }
    public int MaxRange { get; set; }
    public int MaxHeightRangeUp { get; set; } = 2;
    public int MaxHeightRangeDown { get; set; } = 2;
    public int AreaOfEffect { get; set; } = 0;
    public int MaxAoeHeightDelta { get; set; } = 0;

    ///Special bonus things
    //targeting type (line, etc) if we want something other than the radiate out shape pattern
    //affiliation restriction, if we want to make something only impact one type or another
}
