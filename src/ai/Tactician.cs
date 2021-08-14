﻿using Ecs;
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
        // Wait and do nothing
        var defaultPlan = new TacticalPlan()
        {
            MoveTargetLocation = acting.GetComponent<TileLocation>().TilePosition,
            SelectedSkill = null,
            SkillTargetLocation = Vector3.Zero,
            Value = 0
        };

        // Get all the places the acting entity could potentially spend their turn in
        var placesToStand = map.AStar.GetPointsInRange(acting.GetComponent<Movable>(), acting.GetComponent<TileLocation>().TilePosition);

        var bestPlan = acting.GetComponent<SkillSet>().Skills
            .Where(skill => skill.CurrentTP > 0)
            .Select(skill => SelectBestSingleTurnPlanForSkill(skill, acting, placesToStand, map, potentialTargets))
            .Where(plan => plan != null)
            .Where(plan => plan.Value > 0)
            .Aggregate(defaultPlan, (planOne, planTwo) => planOne.Value >= planTwo.Value ? planOne : planTwo);

        if (bestPlan == defaultPlan)
        {
            bestPlan = acting.GetComponent<SkillSet>().Skills
                .Where(skill => skill.CurrentTP > 0)
                .Select(skill => SelectBestMultiTurnPlanForSkill(skill, acting, map, potentialTargets))
                .Where(plan => plan != null)
                .Where(plan => plan.Value > 0)
                .Aggregate(defaultPlan, (planOne, planTwo) => planOne.Value >= planTwo.Value ? planOne : planTwo);
         }

        return bestPlan;
    }

    private static TacticalPlan SelectBestSingleTurnPlanForSkill(Skill skill, Entity acting, List<Vector3> placesToStand, Map map, IEnumerable<Entity> potentialTargets)
    {
        GD.Print($"Selecting best plan for {skill.Name}");
        var actingLocationComp = acting.GetComponent<TileLocation>();
        var startingPosition = actingLocationComp.TilePosition;
        var movable = acting.GetComponent<Movable>();

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
        //  TODO: BUG: Small bug at least, but because we don't take into account the move distance in valuation, we can do a suboptimal
        //      play for AOE. Ex. To avoid hitting self, move one space and throw bomb, instead of just throwing the bomb further away
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
        }).ToList();

        GD.Print($"Narrowed it down to the best places to stand for each of those skill landing spots");
        GD.Print($"There are a total of {targetStandPairs.Count()} pairs of places you could stand and throw now");

        // For each unique place a skill could land, create a list of lists of targeteds using targetutils
        //  potential optimization if we do this first but filter out any complete misses?
        // TODO: BUG: If a move can self target and has a range more than 0, the option to move and use it on self will
        //      never appear, because with a range of 1+, it could be used from somewhere closer. But since we are evaluating
        //      above based on locations and not targets, it doesn't evaluate "logically"
        //      The way to fix it is to look at targets earlier (but it's gonna be less efficient)
        //      Wait until we Strategist is implemented, since this wouldn't be an "optimal" move in the current simplified logic anyway
        var effectsOfSkillAtTargets = targetStandPairs.Select(tsp =>
        {
            // Temporarily set the actors tile position to the theoretical standing position
            actingLocationComp.TilePosition = tsp.standPosition;

            var targetLocations = TargetUtils.GetTargetLocations(skill, map, tsp.targetPosition);
            var effects = TargetUtils.GetTargeteds(skill, acting, potentialTargets, targetLocations);

            // Revert the actors tile position to the original
            actingLocationComp.TilePosition = startingPosition;

            return (tsp.targetPosition, tsp.standPosition, effects);
        })
            // TODO: This isn't really NECESSARY I think? See if it's faster to do it or not when optimizing.
            .Where(est => est.effects.Count() > 0)
            .ToList();

        if (effectsOfSkillAtTargets.Count() == 0)
        {
            GD.Print($"No valid skill targets");
            return null;
        }

        GD.Print($"Narrowed it down to {effectsOfSkillAtTargets.Count()} locations where the skill actually does SOMETHING");

        // For each list of targeteds, determine the Value of that
        //  For now, don't take path distance into account when scoring (but we could later)
        var effectValues = effectsOfSkillAtTargets.Select(est => 
            (est.targetPosition, est.standPosition, est.effects, value: est.effects.Sum(fx => DetermineValue(acting, fx))))
            .ToList();

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
    }

    // This might even be able to replace the current system? Except it doesn't do AOEs as well
    private static TacticalPlan SelectBestMultiTurnPlanForSkill(Skill skill, Entity acting, Map map, IEnumerable<Entity> potentialTargets)
    {
        GD.Print($"Looking for a multi turn plan for {skill.Name}");
        var actingLocationComp = acting.GetComponent<TileLocation>();
        var startingPosition = actingLocationComp.TilePosition;
        var movable = acting.GetComponent<Movable>();

        // For each skill, find the value of dropping it dead center on each potential target
        var effectsOfSkillAtTargets = potentialTargets.Select(target =>
        {
            // Not updating the acting unit's stand position as presumably we are over a turn away when doing this
            var targetPosition = target.GetComponent<TileLocation>().TilePosition;
            var targetLocations = TargetUtils.GetTargetLocations(skill, map, targetPosition);
            var effects = TargetUtils.GetTargeteds(skill, acting, potentialTargets, targetLocations);

            return (name: target.GetComponent<ProfileDetails>().Name, targetPosition, effects);
        }).ToList();

        GD.Print($"Got a list of {effectsOfSkillAtTargets.Count()} places to use this skill");

        // Just get a walking path to the target position and assume we'd stop when we're close enough
        //  instead of having to figure out where we can throw from
        var travelAndEffects = effectsOfSkillAtTargets.Select(target =>
        {
            // QQ? Does this work? Since the target position will be an obstacle in pathfinding
            var path = map.AStar.GetPath(movable, startingPosition, target.targetPosition);
            // Very imperfect because it doesn't take into account skills with more vertical range than
            //  our jump heights. This might incorrectly assume it would take longer than needed to
            //  use the skill, but it's all approximations anyway.
            var walkingDistance = path.Length - skill.AreaOfEffect - skill.MaxRange - 1;
            // Doesn't take into account terrain difficulty either :p
            // Integer math on purpose to get a whole number of turns
            var additionalTurnsNeeded = walkingDistance / movable.MaxMove;
            return (path, walkingDistance, additionalTurnsNeeded, target.effects);
        })
            // If turns == 0, that means that we are too close and can't get far enough away to use this skill
            //  (like if we're blocked into a corner or something)
            .Where(tae => tae.additionalTurnsNeeded > 0)
            .ToList();

        GD.Print($"Narrowed it down to { travelAndEffects.Count()} viable places with paths");

        // Value is (normal value / number of turns it would take to get there and do it)
        var effectValues = travelAndEffects.Select(tae =>
            (tae.path, tae.walkingDistance, tae.additionalTurnsNeeded, tae.effects, value: tae.effects.Sum(fx => DetermineValue(acting, fx) / tae.additionalTurnsNeeded)))
            .ToList();

        var highestValue = effectValues.Aggregate((evOne, evTwo) => evOne.value >= evTwo.value ? evOne : evTwo);

        GD.Print($"Calculated scores");

        // If additionalTurnsNeeded == 1, we should travel the minimum distance we need to get there next turn
        //  If additionalTurnsNeeded > 1, travel as far as we can to give us more options next turn
        // This only works if the terrain is all even cost :(
        Vector3 moveTarget;
        if (highestValue.additionalTurnsNeeded == 1)
        {
            moveTarget = highestValue.path[highestValue.walkingDistance - movable.MaxMove];
        }
        else
        {
            moveTarget = highestValue.path[movable.MaxMove];
        }

        GD.Print($"Evaluated the highest value:");
        GD.Print($"Turns to get there {highestValue.additionalTurnsNeeded}");
        GD.Print($"Intermediate move location {moveTarget}");
        GD.Print($"Value {highestValue.value}");
        // Tactical plan is no skill, but move to the spot that sets us up best for a later turn
        return new TacticalPlan()
        {
            MoveTargetLocation = moveTarget,
            SelectedSkill = null,
            SkillTargetLocation = Vector3.Zero,
            Value = highestValue.value
        };
    }

    private static float DetermineValue(Entity acting, (Entity, Targeted) effect)
    {
        var value = 0f;
        var target = effect.Item1;
        var skillEffects = effect.Item2;

        foreach (var kvp in skillEffects.Effects)
        {
            // All values in here we assume are targeting enemies, so positive effects are negative
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
