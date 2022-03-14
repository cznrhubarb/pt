
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
        // TODO: Is this duplicated code with the method below? Maybe just a little.
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
                    var areaRange = selectedSkill.AreaOfEffect;
                    var maxAoeHeightDelta = selectedSkill.MaxAoeHeightDelta;
                    return map.AStar.GetPointsBetweenRange(targetCenter, 0, areaRange)
                        .Where(pt => Math.Abs(pt.z - targetCenter.z) <= maxAoeHeightDelta)
                        .Where(pt => !(selectedSkill.AoeExcludesCenter && pt == targetCenter))
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

        var markedTargets = actualTargets.Select(target =>
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
                            var damage = Math.Ceiling((int)kvp.Value * statMod * (1 + eleMod));
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
                            var damage = Math.Ceiling((int)kvp.Value * statMod * (1 + eleMod));
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
                            var heal = Math.Ceiling((int)kvp.Value * actingFightStats.Mag / 100d * 4);
                            targetedComp.TargetEffects.Add(kvp.Key, (int)heal);
                        }
                        break;
                    case "Push":
                        {
                            // TODO: We should move this into the targeted so we can do the shadow
                            var actingPos = actingEntity.GetComponent<TileLocation>().TilePosition;
                            var targetLocation = target.GetComponent<TileLocation>();
                            var pushDirection = (targetLocation.TilePosition - actingPos).ToDirection();
                            var points = map.AStar.GetPointsInLine(targetLocation.TilePosition, pushDirection, (int)kvp.Value);
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
                            targetedComp.TargetEffects.Add(kvp.Key, newPos);
                        }
                        break;
                    case "Pull":
                        {
                            // ideally we should be showing a shadow image of where the new locations are for everyone targeted.
                            var actingPos = actingEntity.GetComponent<TileLocation>().TilePosition;
                            var targetLocation = target.GetComponent<TileLocation>();
                            var pullDirection = (actingPos - targetLocation.TilePosition).ToDirection();
                            var points = map.AStar.GetPointsInLine(targetLocation.TilePosition, pullDirection, (int)kvp.Value);
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
                            targetedComp.TargetEffects.Add(kvp.Key, newPos);
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
                        targetedComp.TargetEffects.Add(kvp.Key, kvp.Value);
                        break;
                }
            }

            foreach (var kvp in selectedSkill.SelfEffects)
            {
                switch (kvp.Key)
                {
                    case "Move":
                        // Need a shadow display
                        var actingLocation = actingEntity.GetComponent<TileLocation>();
                        var newPos = actingLocation.TilePosition;
                        if (kvp.Value.Equals("ToTarget"))
                        {
                            newPos = targetCenter;
                        }
                        else if (kvp.Value.Equals("ToAdjacent"))
                        {
                            var targetPos = target.GetComponent<TileLocation>().TilePosition;
                            var direction = (actingLocation.TilePosition - targetPos).ToDirection();
                            newPos = map.AStar.GetPointsInLine(targetPos, direction, 1).First();
                        }

                        targetedComp.UserEffects.Add(kvp.Key, newPos);
                        break;
                }
            }

            return new TargetingOutcomePair() { Entity = target, Outcome = targetedComp };
        }).ToList();

        return markedTargets;
    }

    public static void PerformAction(Manager manager, Entity acting, IEnumerable<Entity> targets)
    {
        foreach (var target in targets)
        {
            var targetedComp = target.GetComponent<Targeted>();
            var targetStatuses = target.GetComponent<StatusBag>().Statuses;

            var roll = Globals.Random.Next(100);
            GD.Print($"Are ya hitting son? {roll}/{targetedComp.HitChance}");
            if (roll < targetedComp.HitChance)
            {
                GD.Print($"HIT {roll}/{targetedComp.HitChance}");
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
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, damage.ToString(), new Color(0.9f, 0.2f, 0.4f));
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
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, kvp.Value.ToString(), new Color(0.9f, 0.2f, 0.4f));
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
                                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, kvp.Value.ToString(), new Color(0.5f, 0.9f, 0.3f));
                            }
                            break;
                        case "Push":
                        case "Pull":
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
                                target.GetComponent<StatusBag>().Statuses.Remove(kvp.Key.TrimStart('-'));
                            }
                            else
                            {
                                target.GetComponent<StatusBag>().Statuses.Add(kvp.Key, StatusFactory.BuildStatusEffect(kvp.Key, (int)kvp.Value));
                                textToDisplay = "+" + textToDisplay;
                            }
                            FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, textToDisplay, new Color(0.5f, 0.4f, 1));
                            break;
                    }
                }
            }
            else
            {
                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, "MISS", new Color(0.7f, 0.6f, 0.6f));
            }

            // Self effects happen even if there is a miss.
            foreach (var kvp in targetedComp.UserEffects)
            {
                switch (kvp.Key)
                {
                    case "Move":
                        {
                            // TODO: Tween to this pos instead of jumping
                            acting.GetComponent<TileLocation>().TilePosition = (Vector3)kvp.Value;
                        }
                        break;
                }
            }
        }
    }

    public class TargetingOutcomePair
    {
        public Entity Entity { get; set; }
        public Targeted Outcome { get; set; }
    }

    // TODO: When targeting string rendering is moved to its own system, this code should also move there.
    public static string BuildTargetingString(Targeted targeted)
    {
        if (targeted == null)
        {
            return "";
        }

        string targetingString = $"{Math.Min(100, targeted.HitChance)}%";
        foreach (var kvp in targeted.TargetEffects)
        {
            if (kvp.Value != null)
            {
                targetingString += $"   {kvp.Value} {kvp.Key}";
            }
            else
            {
                targetingString += $"   {kvp.Key}";
            }
        }

        return targetingString;
    }
}