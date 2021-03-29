using Ecs;
using System.Collections.Generic;

public class MoveSet : Component
{
    public List<Move> Moves { get; set; }
}

public class Move
{
    public string Name { get; set; }
    public int CurrentTP { get; set; }
    public int MaxTP { get; set; }
    public int MinRange { get; set; }
    public int MaxRange { get; set; }
    public int AreaOfEffect { get; set; }

    ///Special bonus things
    //targeting type (line, etc) if we want something other than the radiate out shape pattern
    //affiliation restriction, if we want to make something only impact one type or another
}
