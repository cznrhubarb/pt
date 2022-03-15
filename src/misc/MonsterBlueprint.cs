using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

// Represents the general species info for the monster, used to generate actual instances
[RegisteredType(nameof(MonsterBlueprint), "res://editoricons/Component.svg", nameof(Resource))]
public class MonsterBlueprint : Resource
{
    // Static things
    [Export]
    public Texture ProfilePicture { get; set; } = null;

    [Export]
    public Texture Sprite { get; set; } = null;

    // TODO: Maybe also animations and whatever?

    [Export]
    public int MonNumber { get; set; } = 1;

    [Export]
    public string Name { get; set; } = "";

    [Export]
    public RarityValues Rarity { get; set; } = RarityValues.Common;

    [Export]
    public Element Element { get; set; } = Element.Neutral;

    // Changes with level, but not individual
    [Export]
    public IDictionary<int, Skill> SkillsAvailableByLevel { get; set; } = new Dictionary<int, Skill>();

    [Export]
    public IDictionary<int, Movable> MoveStatsByLevel { get; set; } = new Dictionary<int, Movable>();

    // Each of these values represent the base stat when the monster is at level 100
    [Export]
    public StatBundle Base { get; set; } = new StatBundle();

    // These represent the amount of Partnership growth they give to
    //  other members of their team at the end of a successful combat.
    // One point here correlates to the same as one base state point.
    [Export]
    public StatBundle PartnershipGrowth { get; set; } = new StatBundle();

    // TODO: Evolution paths and triggers
}