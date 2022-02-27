using System;
using System.Collections.Generic;

// Represents an actual monster at a fixed point in time,
//  used to generate components for combat and to serialize for save states
public class MonsterState
{
    // Maybe better off as a filepath?
    // This is where we get element, number, move stats, textures, etc
    public MonsterBlueprint Blueprint { get; set; }

    private string customName = null;
    public string CustomName
    {
        get => customName ?? Blueprint.Name;
        set => customName = value;
    }

    public int Level { get; set; }
    public int Experience { get; set; }

    public List<Skill> Skills { get; set; } = new List<Skill>();

    public StatBundle EffectiveStats { get; set; } = new StatBundle();
    public int MaxHealth { get => EffectiveStats.Health; }
    public int Atn { get => EffectiveStats.Atn; }
    public int Dex { get => EffectiveStats.Dex; }
    public int Mag { get => EffectiveStats.Mag; }
    public int Str { get => EffectiveStats.Str; }
    public int Tuf { get => EffectiveStats.Tuf; }

    public StatBundle Partnership { get; set; } = new StatBundle();

    public StatBundle Genetics { get; set; } = new StatBundle();

    public void Save() { throw new NotImplementedException(); }
    public void Load() { throw new NotImplementedException(); }

    public void RecalculateStats()
    {
        EffectiveStats = (Blueprint.Base + Partnership + Genetics) * Level / 100;
    }

    public StatBundle DetermineStatGainFromLevelUp(int levelsGained)
    {
        var valuesAtLevelUp = (Blueprint.Base + Partnership + Genetics) * (Level + levelsGained) / 100;

        return valuesAtLevelUp - EffectiveStats;
    }
}