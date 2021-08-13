using Ecs;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TacticalPlan
{
    public Vector3 MoveTargetLocation { get; set; }
    public Skill SelectedSkill { get; set; }
    public Vector3 SkillTargetLocation { get; set; }
    public float Value { get; set; }
}

public class Tactician
{
    public static TacticalPlan GetTurnPlan(Entity acting, Map map, IEnumerable<Entity> potentialTargets)
    {
        // TODO: Handle no skills
        // TODO: Handle no skills with a valid target/score (planning over multiple turns)
        // TODO: Don't use a skill with a negative score even if it's the "highest"
        return acting.GetComponent<SkillSet>().Skills
            .Where(skill => skill.CurrentTP > 0)
            .Select(skill => SelectBestPlanForSkill(skill, acting, map, potentialTargets))
            .Aggregate((planOne, planTwo) => planOne.Value >= planTwo.Value ? planOne : planTwo);
    }

    private static TacticalPlan SelectBestPlanForSkill(Skill skill, Entity acting, Map map, IEnumerable<Entity> potentialTargets)
    {
        GD.Print($"Selecting best plan for {skill.Name}");
        var startingPosition = acting.GetComponent<TileLocation>().TilePosition;
        var movable = acting.GetComponent<Movable>();

        // Get all the places the acting entity could potentially spend their turn in
        // TODO: This can be moved outside of this method to optimize it
        var placesToStand = map.AStar.GetPointsInRange(movable, startingPosition);

        GD.Print($"Evaluating skill throwing from {placesToStand.Count} points");

        // For each place the entity could stand, create a list of places the skill could land
        //  Store them in a dictionary where the skill target location is the key so we can reduce to the best
        var standPointsByTarget = new Dictionary<Vector3, List<Vector3>>();
        foreach (var position in placesToStand)
        {
            var targetPoints = map.AStar.GetPointsBetweenRange(position, skill.MinRange, skill.MaxRange)
                .Where(pt => position.z - pt.z <= skill.MaxHeightRangeDown && pt.z - position.z <= skill.MaxHeightRangeUp);
            foreach (var pt in targetPoints)
            {
                var list = standPointsByTarget.ContainsKey(pt) ? standPointsByTarget[pt] : new List<Vector3>();
                list.Add(position);
                standPointsByTarget[pt] = list;
            }
        }

        GD.Print($"There are a total of {standPointsByTarget.Keys.Count} points that this skill could land");
        GD.Print($"There are a total of {standPointsByTarget.Sum(l => l.Value.Count)} pairs of places you could stand and throw");

        // For each unique place a skill could land, find the shortest distance the acting entity would have to travel to in order to throw it
        //  TODO: for now we'll prefer high ground in case of path length ties, but it could play a larger part since it can affect
        //      both damage output for crits and damage taken from other people critting
        var targetStandPairs = standPointsByTarget.Select(kvp =>
        {
            var paths = kvp.Value.Select(newPosition => (newPosition, map.AStar.GetPath(movable, startingPosition, newPosition).Length));
            var closest = paths.Aggregate((pathOne, pathTwo) => 
            {
                if (pathOne.Length == pathTwo.Length && pathTwo.newPosition.z > pathOne.newPosition.z)
                {
                    return pathTwo;
                }
                else
                {
                    return pathOne.Length <= pathTwo.Length ? pathOne : pathTwo;
                }
            });
            return (targetPosition: kvp.Key, standPosition: closest.newPosition);
        });

        GD.Print($"Narrowed it down to the best places to stand for each of those skill landing spots");
        GD.Print($"There are a total of {targetStandPairs.Count()} pairs of places you could stand and throw now");

        // For each unique place a skill could land, create a list of lists of targeteds using targetutils
        //  potential optimization if we do this first but filter out any complete misses?
        // TODO: BUG: Doesn't treat the new potential standing spot as though it was holding the acting entity
        //      (and likewise it always treats the acting entity like it hasn't moved)
        var effectsOfSkillAtTargets = targetStandPairs.Select(tsp =>
        {
            var targetLocations = TargetUtils.GetTargetLocations(skill, map, tsp.targetPosition);
            var effects = TargetUtils.GetTargeteds(skill, acting, potentialTargets, targetLocations);
            return (tsp.targetPosition, tsp.standPosition, effects);
        });

        GD.Print($"Created all the targeted components to evaluate skills");

        // TODO: This isn't really NECESSARY I think? See if it's faster to do it or not when optimizing.
        effectsOfSkillAtTargets = effectsOfSkillAtTargets.Where(est => est.effects.Count() > 0);

        GD.Print($"Narrowed it down to {effectsOfSkillAtTargets.Count()} locations where the skill actually does SOMETHING");

        // For each list of targeteds, determine the Value of that
        //  For now, don't take path distance into account when scoring (but we could later)
        var effectValues = effectsOfSkillAtTargets.Select(est => 
            (est.targetPosition, est.standPosition, est.effects, value: est.effects.Sum(fx => DetermineValue(acting, fx))));

        GD.Print($"Calculated the total values of throwing each skill at each location");

        // For the highest value targeted, return a tactical plan (maybe including the list of targeteds)
        var highestValue = effectValues.Aggregate((evOne, evTwo) => evOne.value >= evTwo.value ? evOne : evTwo);

        GD.Print($"Evaluated the highest value:");
        GD.Print($"MoveTarget Location {highestValue.standPosition}");
        GD.Print($"SkillTarget Location {highestValue.targetPosition}");
        GD.Print($"Value {highestValue.value}");
        GD.Print("Targets:");
        foreach (var fx in highestValue.effects)
        {
            GD.Print($"  {fx.Item1.GetComponent<ProfileDetails>().Name}");
            foreach (var fx2 in fx.Item2.Effects)
            {
                GD.Print($"    {fx2.Key}: {fx2.Value}");
            }
        }

        return new TacticalPlan()
        {
            MoveTargetLocation = highestValue.standPosition,
            SelectedSkill = skill,
            SkillTargetLocation = highestValue.targetPosition,
            Value = highestValue.value
        };

        // TODO: BUG: It's somewhere in this method, but Double Team isn't targeting correctly.
        //      The score is way off, it's targeting an enemy, the move target location is invalid
        /*
        Selecting best plan for Double Team
        Evaluating skill throwing from 23 points
        There are a total of 23 points that this skill could land
        There are a total of 23 pairs of places you could stand and throw
        Narrowed it down to the best places to stand for each of those skill landing spots
        There are a total of 23 pairs of places you could stand and throw now
        Created all the targeted components to evaluate skills
        Narrowed it down to 1 locations where the skill actually does SOMETHING
        Calculated the total values of throwing each skill at each location
        Evaluated the highest value:
            MoveTarget Location (5, 1, 2)
            SkillTarget Location (5, 1, 2)
            Value -499.95
            Targets:
                Vaporeon
                    Elated: 1
        */
    }

    private static float DetermineValue(Entity acting, (Entity, Targeted) effect)
    {
        var value = 0f;
        var target = effect.Item1;
        var skillEffects = effect.Item2;

        // Ex. Tackle:
        //  Value of hitting that target would be (Damage * ChanceToHit)

        // Ex. Bomb Toss:
        //  Value of hitting would be sum of enemy targets (Damage * ChanceToHit) - sum of friendly targets (Damage * ChanceToHit)
        //      (potentially weight hurting friendly targets at 2x?)

        // Ex. Double Team:
        //  If you don't already have the buff, this has a static value. If you have the buff, it's zero.

        foreach (var kvp in skillEffects.Effects)
        {
            // All values in here we assume are targeting ourselves
            var amount = 0f;
            switch (kvp.Key)
            {
                case "StrDamage":
                    {
                        var healthComp = target.GetComponent<Health>();
                        amount = (int)kvp.Value;
                        amount += skillEffects.CritChance / 100f * amount;
                        amount = Math.Min(healthComp.Current, amount);
                        // TODO: Weight death blows a bit higher, and weight crit a bit less on deathblows since it's less certain
                    }
                    break;
                case "MagDamage":
                    {
                        var healthComp = target.GetComponent<Health>();
                        amount = (int)kvp.Value;
                        amount = Math.Min(healthComp.Current, amount);
                        // TODO: Weight death blows a bit higher
                    }
                    break;
                case "Heal":
                    {
                        var healthComp = target.GetComponent<Health>();
                        amount = (int)kvp.Value;
                        amount = Math.Min(healthComp.Max - healthComp.Current, amount);
                        // Invert positive effects
                        amount = -amount;
                    }
                    break;
                case "Elated":
                    {
                        // TODO: Flat amount, though it really should be based on whether it's relevant
                        //  (IE. don't buff str damage for someone with all mag attacks)
                        // Invert positive effects
                        amount = -5;
                    }
                    break;
                default:
                    GD.Print("Attempted to calculate AI value for unknown skill effect: " + kvp.Key);
                    break;
            }

            value += amount;
        }


        // If we're the same affiliation, make it a negative value and weigh it a bit more
        //  so we're less likely to whack our own team unless it's like... really worth it.
        // Since we're inverting positive effect values above, this also means we're a little
        //  more likely to help our own team than to hurt the other team (We'll see if that's the right choice in testing)
        Affiliation actingAff = acting.GetComponent<ProfileDetails>().Affiliation;
        Affiliation targetAff = target.GetComponent<ProfileDetails>().Affiliation;
        if (actingAff == targetAff)
        {
            value *= -1.5f;
        }

        // Make the actual value proportional to the chance it will land
        value *= skillEffects.HitChance / 100;

        return value;
    }
}
