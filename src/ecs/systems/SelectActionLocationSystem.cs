using Ecs;
using Godot;
using System;
using System.Linq;

public class SelectActionLocationSystem : Ecs.System
{
    private const string SelectedEntityKey = "selected";
    private const string ActionLocationEntityKey = "actionLocation";
    private const string MapEntityKey = "map";
    private const string TargetedKey = "targeted";

    public SelectActionLocationSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TargetLocation>(ActionLocationEntityKey);
        AddRequiredComponent<TileLocation>(ActionLocationEntityKey);
        AddRequiredComponent<Selected>(SelectedEntityKey);
        AddRequiredComponent<FightStats>(SelectedEntityKey);
        AddRequiredComponent<TileLocation>(SelectedEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
        AddRequiredComponent<Targeted>(TargetedKey);
        AddRequiredComponent<FightStats>(TargetedKey);
        AddRequiredComponent<Health>(TargetedKey);
        AddRequiredComponent<TileLocation>(TargetedKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var movingActor = SingleEntityFor(SelectedEntityKey);
        if (movingActor == null)
        {
            return;
        }

        if (Input.IsActionJustPressed("ui_accept"))
        {
            var potentialLocations = EntitiesFor(ActionLocationEntityKey)
                .Where(ent => ent.Visible)
                .Select(ent => ent.GetComponent<TileLocation>());

            var reticleLocationComp = entity.GetComponent<TileLocation>();
            var targetLocation = potentialLocations.FirstOrDefault(
                location =>
                 location.TilePosition == reticleLocationComp.TilePosition);

            if (targetLocation != null && movingActor.HasComponent<PlayerCharacter>())
            {
                PerformAction();
            }

            // Little hacky, but we don't want to clear this until they have committed to a turn
            var movableComp = movingActor.GetComponentOrNull<Movable>();
            if (movableComp != null)
            {
                movableComp.StartingLocation = null;
            }
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (manager.CurrentState is PlayerTargetingState ptState)
            {
                movingActor.GetComponent<TurnSpeed>().TimeToAct -= ptState.SelectedMove.Speed;
            }

            manager.ApplyState(new PlayerMovementState(movingActor, SingleEntityFor(MapEntityKey)));
        }
    }

    private void PerformAction()
    {
        // TODO: This could potentially be done somewhere else with an event
        if (manager.CurrentState is PlayerTargetingState ptState)
        { 
            var actingEntity = SingleEntityFor(SelectedEntityKey);
            var actingFightStats = actingEntity.GetComponent<FightStats>();

            var selectedMove = ptState.SelectedMove;
            var targets = EntitiesFor(TargetedKey);
            GD.Print($"In PT State with {targets.Count} targets");
            foreach (var target in targets)
            {
                var targetFightStats = target.GetComponent<FightStats>();
                var chanceToHit = Mathf.Floor(selectedMove.Accuracy * Mathf.Pow(2, (actingFightStats.Dex - targetFightStats.Dex) / 20f));
                GD.Print("Chance to hit: " + chanceToHit);

                var roll = Globals.Random.Next(100);
                if (roll < chanceToHit)
                {
                    GD.Print("HIT");
                    var effects = selectedMove.Effects;
                    foreach (var kvp in effects)
                    {
                        switch (kvp.Key)
                        {
                            case "StrDamage":
                                {
                                    var heightDelta = actingEntity.GetComponent<TileLocation>().TilePosition.z -
                                                        target.GetComponent<TileLocation>().TilePosition.z;
                                    var critChance = actingFightStats.Dex / 2 + Mathf.Clamp(heightDelta, -4, 4) * 5 + selectedMove.CritModifier;

                                    var statMod = Math.Pow(1.25f, (actingFightStats.Str - targetFightStats.Tuf) / 20);
                                    var baseEleMod = 0; // or whatever
                                    var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                                    var damage = kvp.Value * statMod * (1 + eleMod);

                                    roll = Globals.Random.Next(100);
                                    if (roll < critChance)
                                    {
                                        GD.Print("CRIT");
                                        damage *= 2;
                                    }

                                    GD.Print($"Str Damage: {damage}");
                                    var healthComp = target.GetComponent<Health>();
                                    healthComp.Current -= Math.Min(healthComp.Current, (int)Math.Ceiling(damage));
                                }
                                break;
                            case "MagDamage":
                                {
                                    var statMod = Math.Pow(1.25f, (actingFightStats.Mag - targetFightStats.Tuf) / 20);
                                    var baseEleMod = 0; // or whatever
                                    var eleMod = (actingFightStats.Atn + targetFightStats.Atn) / 100 * baseEleMod;
                                    var damage = kvp.Value * statMod * (1 + eleMod);
                                    GD.Print($"Mag Damage: {damage}");
                                    var healthComp = target.GetComponent<Health>();
                                    healthComp.Current -= Math.Min(healthComp.Current, (int)Math.Ceiling(damage));
                                }
                                break;
                            case "Heal":
                                {
                                    var healthComp = target.GetComponent<Health>();
                                    var heal = (int)(kvp.Value * actingFightStats.Mag / 100f * 4);
                                    healthComp.Current = Math.Max(healthComp.Max, healthComp.Current + heal);
                                    GD.Print($"Str Damage: {heal}");
                                }
                                break;
                            case "BoostStrength":
                                GD.Print("BoostStrength");
                                break;
                            default:
                                GD.Print("Attempted to apply unknown move effect: " + kvp.Key);
                                break;
                        }
                    }
                }
                else
                {
                    GD.Print("MISS");
                }
            }
        }

        manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
    }
}
