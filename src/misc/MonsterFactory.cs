
using Ecs;
using System;
using System.Collections.Generic;
using System.Linq;

public class MonsterFactory
{
    public static MonsterState BuildMonster(MonsterBlueprint blueprint, int level)
    {
        var monsterState = new MonsterState
        {
            Blueprint = blueprint,
            Level = level,
            Experience = 90,
            Partnership = new StatBundle(),
            Genetics = new StatBundle()
            {
                Health = Globals.Random.Next(0, 50),
                Atn = Globals.Random.Next(0, 50),
                Dex = Globals.Random.Next(0, 50),
                Mag = Globals.Random.Next(0, 50),
                Str = Globals.Random.Next(0, 50),
                Tuf = Globals.Random.Next(0, 50)
            }
        };

        var lastFourSkills = blueprint.SkillLearnset
            .LevelSkills
            .Where(skill => skill.levelLearned <= level)
            .OrderByDescending(skill => skill.levelLearned)
            .Take(4)
            .Select(skill => skill.skill.Clone());
        monsterState.Skills.AddRange(lastFourSkills);

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

        var movable = blueprint.MoveStatsByLevel
            .Where(skill => skill.Key <= monsterState.Level)
            .OrderByDescending(skill => skill.Key)
            .Select(skill => skill.Value)
            .First()
            .Clone();
        components.Add(movable);

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
        var avgSkillSpeed = monsterState.Skills.Sum(skill => skill.Speed) / monsterState.Skills.Count;
        components.Add(new TurnSpeed() { TimeToAct = avgSkillSpeed + movable.TravelSpeed });
        components.Add(new Rarity() { Value = blueprint.Rarity });

        return components;
    }

    public static MonsterState LevelUp(MonsterState monster)
    {
        throw new NotImplementedException();
    }
}
