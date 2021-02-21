using Ecs;
using System.Collections.Generic;

public class MoveSet : Component
{
    public List<Move> Moves { get; set; }
}

public class Move
{
    public string Name { get; set; }
    // public object Target { get; set; }
    // public string Event { get; set; }
    public int CurrentTP { get; set; }
    public int MaxTP { get; set; }
}
