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
        AddRequiredComponent<StatusBag>(TargetedKey);
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
                if (manager.CurrentState is PlayerTargetingState ptState)
                {
                    ptState.SelectedMove.CurrentTP--;
                    PerformAction();
                }
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
        var targets = EntitiesFor(TargetedKey);
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
                            GD.Print("Attempted to apply unknown move effect: " + kvp.Key);
                            break;
                    }
                }
            }
            else
            {
                FactoryUtils.BuildTextEffect(manager, target.GetComponent<TileLocation>().TilePosition, "MISS", new Color(0.7f, 0.6f, 0.6f));
            }
        }

        manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
    }
}
