using Godot;
using System.Collections.Generic;

// Represents the general species info for the monster, used to generate actual instances
public class MonsterBlueprint : Resource
{
    // Static things
    public Texture ProfilePicture { get; set; } = null;

    public Texture Sprite { get; set; } = null;

    // TODO: Maybe also animations and whatever?

    public int MonNumber { get; set; } = 1;

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public RarityValue Rarity { get; set; } = RarityValue.Common;

    public MonsterType MonsterType { get; set; } = MonsterType.Beast;

    public Element Element { get; set; } = Element.Neutral;

    public SkillLearnset SkillLearnset { get; set; } = new SkillLearnset();

    public IDictionary<int, Movable> MoveStatsByLevel { get; set; } = new Dictionary<int, Movable>();

    // Each of these values represent the base stat when the monster is at level 100
    public StatBundle Base { get; set; } = new StatBundle();

    // These represent the amount of Partnership growth they give to
    //  other members of their team at the end of a successful combat.
    // One point here correlates to the same as one base state point.
    public StatBundle PartnershipGrowth { get; set; } = new StatBundle();

    public List<EvolutionPath> EvolutionPaths = new List<EvolutionPath>();
}