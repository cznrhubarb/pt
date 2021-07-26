using Ecs;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RenderTargetProfileSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";
    private const string TargetedKey = "targeted";
    private const string SelectedKey = "selected";

    public RenderTargetProfileSystem()
    {
        AddRequiredComponent<TargetIndicator>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Selected>(SelectedKey);
        AddRequiredComponent<TileLocation>(SelectedKey);
        AddRequiredComponent<FightStats>(SelectedKey);
        AddRequiredComponent<TileLocation>(PotentialTargetKey);
        AddRequiredComponent<ProfileDetails>(PotentialTargetKey);
        AddRequiredComponent<FightStats>(PotentialTargetKey);
        AddRequiredComponent<Health>(PotentialTargetKey);
        AddRequiredComponent<Targeted>(TargetedKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        var currentTargets = EntitiesFor(TargetedKey);
        foreach (var target in currentTargets)
        {
            manager.RemoveComponentFromEntity<Targeted>(target);
        }

        var indicators = EntitiesFor(PrimaryEntityKey);
        var potentialTargets = EntitiesFor(PotentialTargetKey);

        var actualTargets = potentialTargets.Where(target => 
            indicators.Any(ind => 
                target.GetComponent<TileLocation>().TilePosition == ind.GetComponent<TileLocation>().TilePosition &&
                ind.GetComponent<SpriteWrap>().Sprite.Visible
            )
        );

        var ptState = manager.CurrentState as PlayerTargetingState;
        if (ptState == null)
        {
            // Happens if a new state gets applied along the way?
            return;
        }

        var selectedMove = ptState.SelectedMove;
        var actingEntity = SingleEntityFor(SelectedKey);
        var actingFightStats = actingEntity.GetComponent<FightStats>();

        foreach (var target in actualTargets)
        {
            var targetedComp = new Targeted();
            var targetFightStats = target.GetComponent<FightStats>();
            targetedComp.HitChance = Mathf.Floor(selectedMove.Accuracy * Mathf.Pow(2, (actingFightStats.Dex - targetFightStats.Dex) / 20f));

            foreach (var kvp in selectedMove.Effects)
            {
                switch (kvp.Key)
                {
                    case "StrDamage":
                        {
                            var heightDelta = actingEntity.GetComponent<TileLocation>().TilePosition.z -
                                                target.GetComponent<TileLocation>().TilePosition.z;
                            targetedComp.CritChance = actingFightStats.Dex / 2 + Mathf.Clamp(heightDelta, -4, 4) * 5 + selectedMove.CritModifier;

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
    }

    private string BuildTargetingString(Targeted targeted)
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

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
