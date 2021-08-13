﻿
using Ecs;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TargetUtils
{
    public static List<Vector3> GetTargetLocations(Skill selectedSkill, Map map, Vector3 targetCenter)
    {
        var areaRange = selectedSkill.AreaOfEffect;
        var maxAoeHeightDelta = selectedSkill.MaxAoeHeightDelta;
        return map.AStar.GetPointsBetweenRange(targetCenter, 0, areaRange)
            .Where(pt => Math.Abs(pt.z - targetCenter.z) <= maxAoeHeightDelta)
            .ToList();
    }

    // TODO: Obviously this is a massive amount of duplicated code with the method below
    // TODO: Also the name is terrible
    // TODO: Also I don't like returning a tuple if I can avoid it
    public static IEnumerable<(Entity, Targeted)> GetTargeteds(
        Skill selectedSkill,
        Entity actingEntity,
        IEnumerable<Entity> potentialTargets,
        IEnumerable<Vector3> skillTargetLocations)
    {
        var actualTargets = potentialTargets.Where(target =>
            skillTargetLocations.Any(stl => target.GetComponent<TileLocation>().TilePosition == stl)
        );

        var actingFightStats = actingEntity.GetComponent<FightStats>();

        return actualTargets.Select(target =>
        {
            var targetedComp = new Targeted();
            var targetFightStats = target.GetComponent<FightStats>();
            targetedComp.HitChance = Mathf.Floor(selectedSkill.Accuracy * Mathf.Pow(2, (actingFightStats.Dex - targetFightStats.Dex) / 20f));

            foreach (var kvp in selectedSkill.Effects)
            {
                switch (kvp.Key)
                {
                    case "StrDamage":
                        {
                            var heightDelta = actingEntity.GetComponent<TileLocation>().TilePosition.z -
                                                target.GetComponent<TileLocation>().TilePosition.z;
                            targetedComp.CritChance = actingFightStats.Dex / 2 + Mathf.Clamp(heightDelta, -4, 4) * 5 + selectedSkill.CritModifier;

                            var statMod = Math.Pow(1.25f, (actingFightStats.Str - targetFightStats.Tuf) / 20);
                            var baseEleMod = 0; // TODO: Calculate
                            var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                            var damage = Math.Ceiling(kvp.Value * statMod * (1 + eleMod));
                            targetedComp.Effects.Add(kvp.Key, (int)damage);
                        }
                        break;
                    case "MagDamage":
                        {
                            var statMod = Math.Pow(1.25f, (actingFightStats.Mag - targetFightStats.Tuf) / 20);
                            var baseEleMod = 0; // TODO: Calculate
                            var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                            var damage = Math.Ceiling(kvp.Value * statMod * (1 + eleMod));
                            targetedComp.Effects.Add(kvp.Key, (int)damage);
                        }
                        break;
                    case "Heal":
                        {
                            var healthComp = target.GetComponent<Health>();
                            var heal = Math.Ceiling(kvp.Value * actingFightStats.Mag / 100d * 4);
                            targetedComp.Effects.Add(kvp.Key, (int)heal);
                        }
                        break;
                    case "Elated":
                        targetedComp.Effects.Add(kvp.Key, 1);
                        break;
                    default:
                        break;
                }
            }

            return (target, targetedComp);
        });
    }

    public static IEnumerable<Entity> MarkTargets(
        Manager manager, 
        Skill selectedSkill,
        Entity actingEntity,
        IEnumerable<Entity> potentialTargets,
        IEnumerable<Vector3> skillTargetLocations)
    {
        var actualTargets = potentialTargets.Where(target =>
            skillTargetLocations.Any(stl => target.GetComponent<TileLocation>().TilePosition == stl)
        );

        var actingFightStats = actingEntity.GetComponent<FightStats>();

        foreach (var target in actualTargets)
        {
            var targetedComp = new Targeted();
            var targetFightStats = target.GetComponent<FightStats>();
            targetedComp.HitChance = Mathf.Floor(selectedSkill.Accuracy * Mathf.Pow(2, (actingFightStats.Dex - targetFightStats.Dex) / 20f));

            foreach (var kvp in selectedSkill.Effects)
            {
                switch (kvp.Key)
                {
                    case "StrDamage":
                        {
                            var heightDelta = actingEntity.GetComponent<TileLocation>().TilePosition.z -
                                                target.GetComponent<TileLocation>().TilePosition.z;
                            targetedComp.CritChance = actingFightStats.Dex / 2 + Mathf.Clamp(heightDelta, -4, 4) * 5 + selectedSkill.CritModifier;

                            var statMod = Math.Pow(1.25f, (actingFightStats.Str - targetFightStats.Tuf) / 20);
                            var baseEleMod = 0; // TODO: Calculate
                            var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                            var damage = Math.Ceiling(kvp.Value * statMod * (1 + eleMod));
                            targetedComp.Effects.Add(kvp.Key, (int)damage);
                        }
                        break;
                    case "MagDamage":
                        {
                            var statMod = Math.Pow(1.25f, (actingFightStats.Mag - targetFightStats.Tuf) / 20);
                            var baseEleMod = 0; // TODO: Calculate
                            var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                            var damage = Math.Ceiling(kvp.Value * statMod * (1 + eleMod));
                            targetedComp.Effects.Add(kvp.Key, (int)damage);
                        }
                        break;
                    case "Heal":
                        {
                            var healthComp = target.GetComponent<Health>();
                            var heal = Math.Ceiling(kvp.Value * actingFightStats.Mag / 100d * 4);
                            targetedComp.Effects.Add(kvp.Key, (int)heal);
                        }
                        break;
                    case "Elated":
                        targetedComp.Effects.Add(kvp.Key, 1);
                        break;
                    default:
                        break;
                }
            }
            manager.AddComponentToEntity(target, targetedComp);
        }

        var firstTarget = actualTargets.FirstOrDefault();
        manager.PerformHudAction("SetTargetingInfo", BuildTargetingString(firstTarget?.GetComponent<Targeted>()));
        manager.PerformHudAction("SetProfile", Direction.Right, firstTarget);
        // TODO: Indicate if there are more than one
        // TODO: Deterministic sort by distance from center
        // TODO: Maybe here, maybe somewhere else, but display an on map indicator of which unit is displaying profile card

        return actualTargets;
    }

    public static void PerformAction(Manager manager, IEnumerable<Entity> targets)
    {
        foreach (var target in targets)
        {
            var targetedComp = target.GetComponent<Targeted>();

            var roll = Globals.Random.Next(100);
            if (roll < targetedComp.HitChance)
            {
                var effects = targetedComp.Effects;
                foreach (var kvp in effects)
                {
                    switch (kvp.Key)
                    {
                        case "StrDamage":
                            {
                                var damage = (int)kvp.Value;
                                roll = Globals.Random.Next(100);
                                if (roll < targetedComp.CritChance)
                                {
                                    GD.Print("CRIT");
                                    damage *= 2;
                                }

                                var healthComp = target.GetComponent<Health>();
                                healthComp.Current -= Math.Min(healthComp.Current, damage);
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, damage.ToString(), new Color(0.9f, 0.2f, 0.4f));
                                if (healthComp.Current == 0)
                                {
                                    manager.AddComponentToEntity(target, new Dying());
                                }
                            }
                            break;
                        case "MagDamage":
                            {
                                var healthComp = target.GetComponent<Health>();
                                healthComp.Current -= Math.Min(healthComp.Current, (int)kvp.Value);
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, kvp.Value.ToString(), new Color(0.9f, 0.2f, 0.4f));
                                if (healthComp.Current == 0)
                                {
                                    manager.AddComponentToEntity(target, new Dying());
                                }
                            }
                            break;
                        case "Heal":
                            {
                                var healthComp = target.GetComponent<Health>();
                                healthComp.Current = Math.Max(healthComp.Max, healthComp.Current + (int)kvp.Value);
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, kvp.Value.ToString(), new Color(0.5f, 0.9f, 0.3f));
                            }
                            break;
                        case "Elated":
                            {
                                target.GetComponent<StatusBag>().StatusList.Add(new StatusEffect() { Name = kvp.Key, Count = (int)kvp.Value, Positive = true, Ticks = false });
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, "+BOOST STR", new Color(0.5f, 0.4f, 1));
                            }
                            break;
                        default:
                            GD.Print("Attempted to apply unknown skill effect: " + kvp.Key);
                            break;
                    }
                }
            }
            else
            {
                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, "MISS", new Color(0.7f, 0.6f, 0.6f));
            }
        }
    }

    private static string BuildTargetingString(Targeted targeted)
    {
        if (targeted == null)
        {
            return "";
        }

        string targetingString = $"{Math.Min(100, targeted.HitChance)}%";
        foreach (var kvp in targeted.Effects)
        {
            targetingString += $"   {kvp.Value} {kvp.Key}";
        }

        return targetingString;
    }
}