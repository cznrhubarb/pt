using Godot;
using System.Collections.Generic;

public class Skill : Resource
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Element Element { get; set; } = Element.Neutral;
    public int CurrentTP { get; set; }
    public int MaxTP { get; set; } = 10;
    public TargetingMode TargetingMode { get; set; } = TargetingMode.StandardArea;
    public int MinRange { get; set; } = 1;
    public int MaxRange { get; set; } = 1;
    public int MaxHeightRangeUp { get; set; } = 2;
    public int MaxHeightRangeDown { get; set; } = 2;
    public int MinAoeRange { get; set; } = 0;
    public int MaxAoeRange { get; set; } = 0;
    // HACK: This is used because movement skills look at the pre-movement state.
    //  (Which I guess is correct for some moves and not others.)
    //  Probably OK to leave this hack in long term I think?
    public bool IgnoreUser { get; set; } = false;
    public int MaxAoeHeightDelta { get; set; } = 0;
    // Lower is better
    public int Speed { get; set; } = 0;
    // TODO: Is Physical duplicative of StrDamage vs MagDamage?
    public bool Physical { get; set; } = true;

    public int Accuracy { get; set; } = 50;
    public int CritModifier { get; set; } = 0;

    public Dictionary<string, string> TargetEffects { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, string> SelfEffects { get; set; } = new Dictionary<string, string>();

    ///Special bonus things
    //targeting type (line, etc) if we want something other than the radiate out shape pattern
    //affiliation restriction, if we want to make something only impact one type or another

    public Skill Clone()
    {
        return new Skill()
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Element = Element,
            MaxTP = MaxTP,
            TargetingMode = TargetingMode,
            MinRange = MinRange,
            MaxRange = MaxRange,
            MaxHeightRangeUp = MaxHeightRangeUp,
            MaxHeightRangeDown = MaxHeightRangeDown,
            MinAoeRange = MinAoeRange,
            MaxAoeRange = MaxAoeRange,
            IgnoreUser = IgnoreUser,
            MaxAoeHeightDelta = MaxAoeHeightDelta,
            Speed = Speed,
            Physical = Physical,
            Accuracy = Accuracy,
            CritModifier = CritModifier,
            // These are shallow copies, but it should be OK
            TargetEffects = TargetEffects,
            SelfEffects = SelfEffects
        };
    }
}
