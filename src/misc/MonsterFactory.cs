
using Ecs;
using Godot;
using System;
using System.Collections.Generic;

public class MonsterFactory
{
    public static List<Component> BuildMonster(MonsterBlueprint blueprint, int level)
    {
        var components = new List<Component>();

        components.Add(new ProfileDetails()
        {
            Level = level,
            MonNumber = blueprint.MonNumber,
            Name = blueprint.Name,
        });
        components.Add(new Elemental() { Element = blueprint.Element });

        var skillList = new List<Skill>();
        var levelIter = level;
        // TODO: There's a faster way to iterate through these :p
        while (levelIter > 0 && skillList.Count < 4)
        {
            if (blueprint.SkillsAvailableByLevel.ContainsKey(levelIter))
            {
                skillList.Add(blueprint.SkillsAvailableByLevel[levelIter].Duplicate() as Skill);
            }
            levelIter--;
        }
        components.Add(new SkillSet() { Skills = skillList });

        levelIter = level;
        // TODO: There's a faster way to iterate through these :p
        while (levelIter > 0)
        {
            if (blueprint.MoveStatsByLevel.ContainsKey(levelIter))
            {
                components.Add(blueprint.MoveStatsByLevel[levelIter].Duplicate() as Movable);
            }
            levelIter--;
        }

        // TODO: Determine growth for level and uniquness modifiers
        var maxHealth = blueprint.BaseMaxHealth;
        components.Add(new Health() { Current = maxHealth, Max = maxHealth });

        // TODO: Determine growth for level and uniquness modifiers
        var atn = blueprint.BaseAtn;
        var mag = blueprint.BaseMag;
        var str = blueprint.BaseStr;
        var dex = blueprint.BaseDex;
        var tuf = blueprint.BaseTuf;
        components.Add(new FightStats()
        {
            Atn = atn,
            Mag = mag,
            Str = str,
            Dex = dex,
            Tuf = tuf
        });

        return components;
    }

    public static void LevelUp(Entity monster)
    {
        throw new NotImplementedException();
    }
}
