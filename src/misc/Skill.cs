using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

[RegisteredType(nameof(Skill), "res://editoricons/Component.svg", nameof(Resource))]
public class Skill : Resource
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
    // Lower is better
    public int Speed { get; set; } = 0;
    public bool Physical { get; set; } = true;

    public int Accuracy { get; set; }
    public int CritModifier { get; set; } = 0;

    public Dictionary<string, int> Effects { get; set; }

    ///Special bonus things
    //targeting type (line, etc) if we want something other than the radiate out shape pattern
    //affiliation restriction, if we want to make something only impact one type or another
}
