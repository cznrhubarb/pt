
using Ecs;
using Godot;
using System;
using System.Collections.Generic;

public class MonsterFactory
{
    public static List<Component> BuildMonster(MonsterBlueprint blueprint, int level)
    {
        var components = new List<Component>();

        var profileDetails = blueprint.ProfileDetails.Clone() as ProfileDetails;
        profileDetails.Level = level;
        components.Add(profileDetails);
        components.Add(blueprint.Elemental.Clone() as Component);

        var skillList = new List<Skill>();
        var levelIter = level;
        while (levelIter > 0 && skillList.Count < 4)
        {
            if (blueprint.SkillsAvailableByLevel.ContainsKey(levelIter))
            {
                // TODO: Make sure I don't need to clone this...
                skillList.Add(blueprint.SkillsAvailableByLevel[levelIter]);
            }
            levelIter--;
        }
        components.Add(new SkillSet() { Skills = skillList });

        levelIter = level;
        while (levelIter > 0)
        {
            if (blueprint.MoveStatsByLevel.ContainsKey(levelIter))
            {
                components.Add(blueprint.MoveStatsByLevel[levelIter].Clone() as Movable);
            }
            levelIter--;
        }

        // TODO: Determine growth for level and uniquness modifiers
        var baseHealth = blueprint.BaseHealth.Clone() as Health;
        components.Add(baseHealth);

        // TODO: Determine growth for level and uniquness modifiers
        var baseFightStats = blueprint.BaseFightStats.Clone() as FightStats;
        components.Add(baseFightStats);

        return components;
    }

    public static void LevelUp(Entity monster)
    {
        throw new NotImplementedException();
    }
}
