using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

[RegisteredType(nameof(Skill), "res://editoricons/Component.svg", nameof(Resource))]
public class Skill : Resource
{
    // MIKE_TODO: Remove exports when finished migrating
    public int Id { get; set; } = -1;

    [Export]
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    [Export]
    public Element Element { get; set; } = Element.Neutral;
    public int CurrentTP { get; set; }
    [Export]
    public int MaxTP { get; set; } = 10;
    [Export]
    public TargetingMode TargetingMode { get; set; } = TargetingMode.StandardArea;
    [Export]
    public int MinRange { get; set; } = 1;
    [Export]
    public int MaxRange { get; set; } = 1;
    [Export]
    public int MaxHeightRangeUp { get; set; } = 2;
    [Export]
    public int MaxHeightRangeDown { get; set; } = 2;
    public int MinAoeRange { get; set; } = 0;
    public int MaxAoeRange { get; set; } = 0;
    // HACK: This is used because movement skills look at the pre-movement state.
    //  (Which I guess is correct for some moves and not others.)
    //  Probably OK to leave this hack in long term I think?
    [Export]
    public bool IgnoreUser { get; set; } = false;
    [Export]
    public int MaxAoeHeightDelta { get; set; } = 0;
    // Lower is better
    [Export]
    public int Speed { get; set; } = 0;
    // TODO: Is Physical duplicative of StrDamage vs MagDamage?
    [Export]
    public bool Physical { get; set; } = true;

    [Export]
    public int Accuracy { get; set; } = 50;
    [Export]
    public int CritModifier { get; set; } = 0;

    [Export]
    public Dictionary<string, string> TargetEffects { get; set; } = new Dictionary<string, string>();

    [Export]
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
