
using Ecs;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TargetUtils
{
    public static List<Vector3> GetPotentialTargetLocations(Skill selectedSkill, Map map, Vector3 origin)
    {
        List<Vector3> points = null;
        switch (selectedSkill.TargetingMode)
        {
            case TargetingMode.StandardArea:
                {
                    points = map.AStar.GetPointsBetweenRange(origin, selectedSkill.MinRange, selectedSkill.MaxRange)
                        .Where(pt => origin.z - pt.z <= selectedSkill.MaxHeightRangeDown && pt.z - origin.z <= selectedSkill.MaxHeightRangeUp)
                        .ToList();
                }
                break;
            case TargetingMode.StandardLine:
            case TargetingMode.WholeLine:
                {
                    points = map.AStar.GetPointsInLine(origin, Direction.Up, selectedSkill.MaxRange)
                        .Concat(map.AStar.GetPointsInLine(origin, Direction.Down, selectedSkill.MaxRange))
                        .Concat(map.AStar.GetPointsInLine(origin, Direction.Left, selectedSkill.MaxRange))
                        .Concat(map.AStar.GetPointsInLine(origin, Direction.Right, selectedSkill.MaxRange))
                        .Where(pt => origin.z - pt.z <= selectedSkill.MaxHeightRangeDown && pt.z - origin.z <= selectedSkill.MaxHeightRangeUp)
                        .ToList();
                }
                break;
            case TargetingMode.BlockedLine:
                {
                    Func<Direction, Vector3?> firstObstaclePosInDirection = (dir) => map.AStar.GetPointsInLine(origin, dir, selectedSkill.MaxRange)
                        .TakeWhile(pt => origin.z - pt.z <= selectedSkill.MaxHeightRangeDown && pt.z - origin.z <= selectedSkill.MaxHeightRangeUp)
                        .Cast<Vector3?>()
                        .FirstOrDefault(pt => map.AStar.HasObstacleAtLocation(pt.Value));

                    points = new List<Vector3?>
                    {
                        firstObstaclePosInDirection(Direction.Up),
                        firstObstaclePosInDirection(Direction.Down),
                        firstObstaclePosInDirection(Direction.Left),
                        firstObstaclePosInDirection(Direction.Right),
                    }.Where(pt => pt.HasValue).Select(pt => pt.Value).ToList();
                }
                break;
            default:
                points = new List<Vector3>();
                break;
        }

        return points;
    }

    public static List<Vector3> GetTargetEffectLocations(Skill selectedSkill, Map map, Vector3 origin, Vector3 targetCenter)
    {
        switch (selectedSkill.TargetingMode)
        {
            case TargetingMode.StandardArea:
            case TargetingMode.StandardLine:
            case TargetingMode.BlockedLine:
                {
                    return map.AStar.GetPointsBetweenRange(targetCenter, selectedSkill.MinAoeRange, selectedSkill.MaxAoeRange)
                        .Where(pt => Math.Abs(pt.z - targetCenter.z) <= selectedSkill.MaxAoeHeightDelta)
                        .ToList();
                }
            case TargetingMode.WholeLine:
                {
                    Func<Direction, IEnumerable<Vector3>> getPointsInDirection = (dir) =>
                        map.AStar.GetPointsInLine(origin, dir, selectedSkill.MaxRange)
                        .Where(pt => origin.z - pt.z <= selectedSkill.MaxHeightRangeDown && pt.z - origin.z <= selectedSkill.MaxHeightRangeUp);

                    var targetLine = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right }
                        .Select(getPointsInDirection)
                        .FirstOrDefault(line => line.Contains(targetCenter));

                    return targetLine?.ToList() ?? new List<Vector3>();
                }
            default:
                return new List<Vector3>();
        }
    }

    public static List<TargetingOutcomePair> MarkTargetingOutcomes(
        Manager manager,
        Skill selectedSkill,
        Entity actingEntity,
        IEnumerable<Entity> potentialTargets,
        Vector3 targetCenter,
        IEnumerable<Vector3> skillTargetLocations)
    {
        var map = manager.GetEntitiesWithComponent<Map>().First().GetComponent<Map>();
        var targetingOutcomes = GetTargetingOutcomes(map, selectedSkill, actingEntity, potentialTargets, targetCenter, skillTargetLocations);
        targetingOutcomes.ForEach(to => manager.AddComponentToEntity(to.Entity, to.Outcome));

        return targetingOutcomes;
    }

    public static List<TargetingOutcomePair> GetTargetingOutcomes(
        Map map,
        Skill selectedSkill,
        Entity actingEntity,
        IEnumerable<Entity> potentialTargets,
        Vector3 targetCenter,
        IEnumerable<Vector3> skillTargetLocations)
    {
        var actualTargets = potentialTargets.Where(target =>
            skillTargetLocations.Any(stl => target.GetComponent<TileLocation>().TilePosition == stl)
        ).Where(target =>
            !(selectedSkill.IgnoreUser && target == actingEntity)
        );

        var actingFightStats = actingEntity.GetComponent<FightStats>();

        var targetingOutcomes = actualTargets.Select(target =>
        {
            var targetedComp = new Targeted();
            var targetFightStats = target.GetComponent<FightStats>();
            var targetStatuses = target.GetComponent<StatusBag>().Statuses;
            targetedComp.HitChance = Mathf.Min(100, Mathf.Floor(selectedSkill.Accuracy * Mathf.Pow(2, (actingFightStats.Dex - targetFightStats.Dex) / 20f)));
            if (targetStatuses.ContainsKey("Blind") && selectedSkill.Physical)
            {
                targetedComp.HitChance *= StatusEffect.BlindAccuracyModifier;
            }

            foreach (var kvp in selectedSkill.TargetEffects)
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
                            var damage = Math.Ceiling(int.Parse(kvp.Value) * statMod * (1 + eleMod));
                            if (targetStatuses.ContainsKey("Protect"))
                            {
                                damage *= StatusEffect.ProtectDamageModifier;
                            }
                            targetedComp.TargetEffects.Add(kvp.Key, (int)damage);
                        }
                        break;
                    case "MagDamage":
                        {
                            var statMod = Math.Pow(1.25f, (actingFightStats.Mag - targetFightStats.Tuf) / 20);
                            var baseEleMod = 0; // TODO: Calculate
                            var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                            var damage = Math.Ceiling(int.Parse(kvp.Value) * statMod * (1 + eleMod));
                            if (targetStatuses.ContainsKey("Shell"))
                            {
                                damage *= StatusEffect.ShellDamageModifier;
                            }
                            targetedComp.TargetEffects.Add(kvp.Key, (int)damage);
                        }
                        break;
                    case "Heal":
                        {
                            var healthComp = target.GetComponent<Health>();
                            var heal = Math.Ceiling(int.Parse(kvp.Value) * actingFightStats.Mag / 100d * 4);
                            targetedComp.TargetEffects.Add(kvp.Key, (int)heal);
                        }
                        break;
                    case "Push":
                        {
                            var actingPos = actingEntity.GetComponent<TileLocation>().TilePosition;
                            var targetLocation = target.GetComponent<TileLocation>();
                            var pushDirection = (targetLocation.TilePosition - actingPos).ToDirection();
                            var points = map.AStar.GetPointsInLine(targetLocation.TilePosition, pushDirection, int.Parse(kvp.Value));
                            // Filter out points. We allow any drop, and only a max 1 upwards change
                            var newPos = targetLocation.TilePosition;
                            foreach (var pt in points)
                            {
                                if (pt.z > newPos.z + 1 || map.AStar.HasObstacleAtLocation(pt))
                                {
                                    break;
                                }
                                newPos = pt;
                            }
                            // TODO: Needs to consider impassable (or "impassable" in the sense that move cost is very high) terrains as well
                            targetedComp.TargetEffects.Add("Move", newPos);
                        }
                        break;
                    case "Pull":
                        {
                            // TODO: Ideally we should be showing a shadow image of where the new locations are for everyone targeted.
                            var actingPos = actingEntity.GetComponent<TileLocation>().TilePosition;
                            var targetLocation = target.GetComponent<TileLocation>();
                            var pullDirection = (actingPos - targetLocation.TilePosition).ToDirection();
                            var points = map.AStar.GetPointsInLine(targetLocation.TilePosition, pullDirection, int.Parse(kvp.Value));
                            // Filter out points. We assume if we can target a pull, we are close enough on height delta
                            var newPos = targetLocation.TilePosition;
                            foreach (var pt in points)
                            {
                                if (map.AStar.HasObstacleAtLocation(pt))
                                {
                                    break;
                                }
                                newPos = pt;
                            }
                            // TODO: Needs to consider impassable (or "impassable" in the sense that move cost is very high) terrains as well
                            targetedComp.TargetEffects.Add("Move", newPos);
                        }
                        break;
                    case "CureNegativeStatuses":
                        {
                            targetStatuses
                                .Where(status => !status.Value.Positive)
                                .ToList()
                                .ForEach(status => targetedComp.TargetEffects.Add($"-{status.Key}", null));
                        }
                        break;
                    case "Capture":
                        {
                            if (targetStatuses.ContainsKey("Uncaptureable"))
                            {
                                targetedComp.HitChance = 0;
                            }
                            else
                            {
                                // Override the hit chance for capture
                                var rarity = target.GetComponent<Rarity>().Value;
                                var healthComp = target.GetComponent<Health>();
                                var trainerLevel = actingEntity.GetComponent<ProfileDetails>().Level;

                                var chance = Constants.Capture.RarityMods[rarity];
                                chance += trainerLevel * Constants.Capture.TrainerLevelMod;
                                chance += Constants.Capture.StatusMods.Max(sm => targetStatuses.ContainsKey(sm.Key) ? sm.Value : 0f);
                                var healthPercent = Mathf.Clamp(healthComp.Current / healthComp.Max, Constants.Capture.HealthFloor, Constants.Capture.HealthCeiling);
                                var interpolatedHealth = (healthPercent - Constants.Capture.HealthFloor) / (Constants.Capture.HealthCeiling - Constants.Capture.HealthFloor);
                                var healthMod = interpolatedHealth * Constants.Capture.HealthCeilingValue + (1 - interpolatedHealth * Constants.Capture.HealthFloorValue);
                                chance += healthMod;
                                targetedComp.HitChance = chance;
                            }
                            targetedComp.TargetEffects.Add("Capture", null);
                        }
                        break;
                    default:
                        // Assume anything not explicitly listed is a status effect
                        targetedComp.TargetEffects.Add(kvp.Key, int.Parse(kvp.Value));
                        break;
                }
            }

            return new TargetingOutcomePair() { Entity = target, Outcome = targetedComp };
        }).ToList();

        // This makes sure we're actually on a spot we can target
        if (skillTargetLocations.Count() > 0)
        {
            var selfTargetedComp = new Targeted() { HitChance = 9999, CritChance = 0 };
            foreach (var kvp in selectedSkill.SelfEffects)
            {
                switch (kvp.Key)
                {
                    case "Move":
                        // TODO: Need a shadow display
                        var actingLocation = actingEntity.GetComponent<TileLocation>();
                        var newPos = actingLocation.TilePosition;
                        if (kvp.Value.Equals("ToTarget"))
                        {
                            newPos = targetCenter;
                        }
                        else if (kvp.Value.Equals("ToAdjacent"))
                        {
                            // ToAdjacent assumes we only ever have one target, because otherwise what
                            //  do we mean adjacent to?
                            var target = actualTargets.FirstOrDefault();
                            if (target == null)
                            {
                                continue;
                            }
                            var targetPos = target.GetComponent<TileLocation>().TilePosition;
                            var direction = (actingLocation.TilePosition - targetPos).ToDirection();
                            newPos = map.AStar.GetPointsInLine(targetPos, direction, 1).First();
                        }

                        selfTargetedComp.TargetEffects.Add("Move", newPos);
                        break;
                }
            }

            if (selfTargetedComp.TargetEffects.Count > 0)
            {
                targetingOutcomes.Add(new TargetingOutcomePair() { Entity = actingEntity, Outcome = selfTargetedComp });
            }
        }

        return targetingOutcomes;
    }

    public static void PerformAction(Manager manager, Entity acting, IEnumerable<Entity> targets)
    {
        foreach (var target in targets)
        {
            var targetedComp = target.GetComponent<Targeted>();
            var targetStatuses = target.GetComponent<StatusBag>().Statuses;

            var roll = Globals.Random.Next(100);
            //GD.Print($"Are ya hitting son? {roll}/{targetedComp.HitChance}");
            if (roll < targetedComp.HitChance)
            {
                var textEffectDelay = 0f;
                foreach (var kvp in targetedComp.TargetEffects)
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
                                FactoryUtils.BuildTextEffect(manager, target, damage.ToString(), new Color(0.9f, 0.2f, 0.4f), textEffectDelay);
                                textEffectDelay += FactoryUtils.TextEffectDelay;
                                targetStatuses.Remove("Sleep");
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
                                FactoryUtils.BuildTextEffect(manager, target, kvp.Value.ToString(), new Color(0.9f, 0.2f, 0.4f), textEffectDelay);
                                textEffectDelay += FactoryUtils.TextEffectDelay;
                                targetStatuses.Remove("Sleep");
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
                                FactoryUtils.BuildTextEffect(manager, target, kvp.Value.ToString(), new Color(0.5f, 0.9f, 0.3f), textEffectDelay);
                                textEffectDelay += FactoryUtils.TextEffectDelay;
                            }
                            break;
                        case "Move":
                            {
                                // TODO: Tween to this pos instead of jumping
                                target.GetComponent<TileLocation>().TilePosition = (Vector3)kvp.Value;
                            }
                            break;
                        case "Capture":
                            {
                                // TODO: Add capture stasis status effect to both
                                //        Implement capture stasis status effect
                                //        Implement end of combat check for this
                            }
                            break;
                        default:
                            // Assume anything not explicitly listed is a status effect
                            // TODO: Apply these better depending on the type. Some stack, some don't
                            if (kvp.Key == "Captured")
                            {
                                manager.AddComponentToEntity(target, new Captured());
                            }

                            var textToDisplay = kvp.Key;
                            if (kvp.Key.StartsWith("-"))
                            {
                                targetStatuses.Remove(kvp.Key.TrimStart('-'));
                            }
                            else
                            {
                                if (targetStatuses.ContainsKey(kvp.Key)) {
                                    if (targetStatuses[kvp.Key].Stacks) {
                                        targetStatuses[kvp.Key].Count += (int)kvp.Value;
                                    } else {
                                        // If a status effect does not stack, we cannot apply it until the previous effect is gone,
                                        //  even if the new amount would be greater. This prevents things like perma stun lock.
                                        break;
                                    }
                                } else {
                                    target.GetComponent<StatusBag>().Statuses.Add(kvp.Key, StatusFactory.BuildStatusEffect(kvp.Key, (int)kvp.Value));
                                }
                                textToDisplay = "+" + textToDisplay;
                            }
                            FactoryUtils.BuildTextEffect(manager, target, textToDisplay, new Color(0.5f, 0.4f, 1), textEffectDelay);
                            textEffectDelay += FactoryUtils.TextEffectDelay;
                            break;
                    }
                }
            }
            else
            {
                FactoryUtils.BuildTextEffect(manager, target, "MISS", new Color(0.7f, 0.6f, 0.6f));
            }
        }
    }

    public class TargetingOutcomePair
    {
        public Entity Entity { get; set; }
        public Targeted Outcome { get; set; }
    }
}