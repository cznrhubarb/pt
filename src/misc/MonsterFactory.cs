
using Ecs;
using Godot;
using System;
using System.Collections.Generic;

public class MonsterFactory
{
    public static MonsterState BuildMonster(MonsterBlueprint blueprint, int level)
    {
        var monsterState = new MonsterState();
        monsterState.Blueprint = blueprint;
        monsterState.Level = level;
        monsterState.CustomName = blueprint.Name;

        var levelIter = level;
        // TODO: There's a faster way to iterate through these :p
        while (levelIter > 0 && monsterState.Skills.Count < 4)
        {
            if (blueprint.SkillsAvailableByLevel.ContainsKey(levelIter))
            {
                monsterState.Skills.Add(blueprint.SkillsAvailableByLevel[levelIter].Duplicate() as Skill);
            }
            levelIter--;
        }

        // TODO: Determine growth for level and uniquness modifiers
        var maxHealth = blueprint.BaseMaxHealth;
        monsterState.MaxHealth = maxHealth;

        // TODO: Determine growth for level and uniquness modifiers
        var atn = blueprint.BaseAtn;
        var mag = blueprint.BaseMag;
        var str = blueprint.BaseStr;
        var dex = blueprint.BaseDex;
        var tuf = blueprint.BaseTuf;
        monsterState.Atn = atn;
        monsterState.Mag = mag;
        monsterState.Str = str;
        monsterState.Dex = dex;
        monsterState.Tuf = tuf;

        return monsterState;
    }

    public static List<Component> GenerateComponents(MonsterState monsterState)
    {
        var components = new List<Component>();
        var blueprint = monsterState.Blueprint;

        components.Add(new ProfileDetails()
        {
            Level = monsterState.Level,
            MonNumber = blueprint.MonNumber,
            Name = monsterState.CustomName,
        });
        components.Add(new Elemental() { Element = blueprint.Element });

        // TODO: Probably need to set TP to max here
        components.Add(new SkillSet() { Skills = monsterState.Skills });

        var levelIter = monsterState.Level;
        // TODO: There's a faster way to iterate through these :p
        while (levelIter > 0)
        {
            if (blueprint.MoveStatsByLevel.ContainsKey(levelIter))
            {
                components.Add(blueprint.MoveStatsByLevel[levelIter].Duplicate() as Movable);
            }
            levelIter--;
        }

        components.Add(new Health() { Current = monsterState.MaxHealth, Max = monsterState.MaxHealth });

        components.Add(new FightStats()
        {
            Atn = monsterState.Atn,
            Mag = monsterState.Mag,
            Str = monsterState.Str,
            Dex = monsterState.Dex,
            Tuf = monsterState.Tuf
        });

        return components;
    }

    public static MonsterState LevelUp(MonsterState monster)
    {
        throw new NotImplementedException();
    }
}
