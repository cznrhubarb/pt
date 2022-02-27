
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
        monsterState.Experience = 90;

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

        monsterState.Partnership = new StatBundle();

        monsterState.Genetics = new StatBundle()
        {
            Health = Globals.Random.Next(0, 50),
            Atn = Globals.Random.Next(0, 50),
            Dex = Globals.Random.Next(0, 50),
            Mag = Globals.Random.Next(0, 50),
            Str = Globals.Random.Next(0, 50),
            Tuf = Globals.Random.Next(0, 50)
        };

        monsterState.RecalculateStats();

        return monsterState;
    }

    public static List<Component> GenerateComponents(MonsterState monsterState, Affiliation affiliation)
    {
        var components = new List<Component>();
        var blueprint = monsterState.Blueprint;

        components.Add(new ProfileDetails() { MonsterState = monsterState });
        components.Add(new Elemental() { Element = blueprint.Element });

        components.Add(new SkillSet() { Skills = monsterState.Skills });
        foreach (var skill in monsterState.Skills)
        {
            skill.CurrentTP = skill.MaxTP;
        }

        var levelIter = monsterState.Level;
        // TODO: There's a faster way to iterate through these :p
        while (levelIter > 0)
        {
            if (blueprint.MoveStatsByLevel.ContainsKey(levelIter))
            {
                components.Add(blueprint.MoveStatsByLevel[levelIter].Duplicate() as Movable);
                break;
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

        // TODO: This isn't completely accurate because it precludes FriendlyNpcs or NeutralNpcs, but its OK for now
        switch (affiliation)
        {
            case Affiliation.Friendly:
                components.Add(new PlayerCharacter());
                break;
            case Affiliation.Enemy:
                components.Add(new EnemyNpc());
                break;
            case Affiliation.Neutral:
                components.Add(new Obstacle());
                break;
        }

        components.Add(new Affiliated() { Affiliation = affiliation });
        components.Add(new Directionality());
        components.Add(new Selectable());
        components.Add(new StatusBag());
        // TODO: Starting time to act should be based on something else, like the movable move speed or average of skills or something
        components.Add(new TurnSpeed() { TimeToAct = Globals.Random.Next(0, 30) });
        components.Add(new Rarity() { Value = blueprint.Rarity });

        return components;
    }

    public static MonsterState LevelUp(MonsterState monster)
    {
        throw new NotImplementedException();
    }
}
