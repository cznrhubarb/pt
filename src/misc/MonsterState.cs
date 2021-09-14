using System;
using System.Collections.Generic;

// Represents an actual monster at a fixed point in time,
//  used to generate components for combat and to serialize for save states
public class MonsterState
{
    // Maybe better off as a filepath?
    // This is where we get element, number, move stats, textures, etc
    public MonsterBlueprint Blueprint { get; set; }

    public string CustomName { get; set; }

    public int Level { get; set; }
    // plus current experience

    public List<Skill> Skills { get; set; } = new List<Skill>();

    public int MaxHealth { get; set; }

    public int Atn { get; set; }
    public int Dex { get; set; }
    public int Mag { get; set; }
    public int Str { get; set; }
    public int Tuf { get; set; }

    // Plus add in hidden growth stats or whatever we need?

    public void Save() { throw new NotImplementedException(); }
    public void Load() { throw new NotImplementedException(); }
}