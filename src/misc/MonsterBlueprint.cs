using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

// TODO: Need a new icon for this
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
    public Element Element { get; set; } = Element.Neutral;

    // Changes with level, but not individual
    [Export]
    public IDictionary<int, Skill> SkillsAvailableByLevel { get; set; } = new Dictionary<int, Skill>();

    [Export]
    public IDictionary<int, Movable> MoveStatsByLevel { get; set; } = new Dictionary<int, Movable>();

    // Changes with level and individual
    [Export]
    public int BaseMaxHealth { get; set; } = 1;

    [Export]
    public int BaseAtn { get; set; } = 1;
    [Export]
    public int BaseDex { get; set; } = 1;
    [Export]
    public int BaseMag { get; set; } = 1;
    [Export]
    public int BaseStr { get; set; } = 1;
    [Export]
    public int BaseTuf { get; set; } = 1;
}