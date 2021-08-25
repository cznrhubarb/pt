using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

// TODO: Need a new icon for this
[RegisteredType(nameof(MonsterBlueprint), "res://editoricons/Component.svg", nameof(Resource))]
public class MonsterBlueprint : Resource
{
    // Static things
    [Export]
    public ProfileDetails ProfileDetails { get; set; } = new ProfileDetails() { Level = 1, MonNumber = 1, Name = "" };

    [Export]
    public Elemental Elemental { get; set; } = new Elemental() { Element = Element.Neutral };

    // Changes with level, but not individual
    [Export]
    public IDictionary<int, Skill> SkillsAvailableByLevel { get; set; } = new Dictionary<int, Skill>();

    [Export]
    public IDictionary<int, Movable> MoveStatsByLevel { get; set; } = new Dictionary<int, Movable>();

    // Changes with level and individual
    [Export]
    public Health BaseHealth { get; set; } = new Health() { Max = 1 };

    [Export]
    public FightStats BaseFightStats { get; set; } = new FightStats() { Atn = 1, Dex = 1, Mag = 1, Str = 1, Tuf = 1 };
}