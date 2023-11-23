using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class NpcTargetingState : State
{
    private Entity acting;
    private Map map;
    private TacticalPlan plan;

    private List<Entity> targetRangeLocations;
    private List<Entity> targetLocations;

    public NpcTargetingState(Entity acting, Map map, TacticalPlan plan)
    {
        this.acting = acting;
        this.map = map;
        this.plan = plan;
    }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC targeting: " + acting.Name);

        if (plan.SelectedSkill != null)
        {
            manager.PerformHudAction("FlashMove", acting.GetComponent<SkillSet>().Skills.IndexOf(plan.SelectedSkill));

            var tilePosition = acting.GetComponent<TileLocation>().TilePosition;

            var points = map.AStar.GetPointsBetweenRange(tilePosition, plan.SelectedSkill.MinRange, plan.SelectedSkill.MaxRange)
                .Where(pt => tilePosition.z - pt.z <= plan.SelectedSkill.MaxHeightRangeDown && pt.z - tilePosition.z <= plan.SelectedSkill.MaxHeightRangeUp)
                .ToList();

            targetRangeLocations = MapUtils.GenerateTileLocationsForPoints<TargetLocation>(manager, points, "res://img/tiles/image_part_031.png");

            manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
            {
                Callback = () =>
                {
                    foreach (var spot in targetRangeLocations)
                    {
                        manager.DeleteEntity(spot.Id);
                    }

                    plan.SelectedSkill.CurrentTP--;

                    var skillTargetPoints = TargetUtils.GetTargetEffectLocations(plan.SelectedSkill, map, plan.MoveTargetLocation, plan.SkillTargetLocation);

                    if (acting.HasComponent<Directionality>())
                    {
                        acting.GetComponent<Directionality>().Direction =
                            (plan.SkillTargetLocation - tilePosition).ToDirection();
                    }

                    targetLocations = MapUtils.GenerateTileLocationsForPoints<TargetLocation>(manager, skillTargetPoints, "res://img/tiles/image_part_030.png");

                    var potentialTargets = manager.GetEntitiesWithComponent<ProfileDetails>()
                        .Where(ent => ent.HasComponent<TileLocation>() && ent.HasComponent<FightStats>() && ent.HasComponent<Health>());

                    TargetUtils.MarkTargetingOutcomes(manager, plan.SelectedSkill, acting, potentialTargets, plan.SkillTargetLocation, skillTargetPoints);

                    manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
                    {
                        Callback = () =>
                        {
                            acting.GetComponent<TurnSpeed>().TimeToAct += plan.SelectedSkill.Speed;
                            // TODO: Would be better if we could apply these along the way instead of all at once
                            //  that way if we are displaying it, it updates correctly.
                            var statuses = acting.GetComponent<StatusBag>().Statuses;
                            if (statuses.ContainsKey("Haste"))
                            {
                                acting.GetComponent<TurnSpeed>().TimeToAct /= 2;
                            }
                            else if (statuses.ContainsKey("Slow"))
                            {
                                acting.GetComponent<TurnSpeed>().TimeToAct *= 2;
                            }

                            var targetedEntities = manager.GetEntitiesWithComponent<Targeted>();
                            TargetUtils.PerformAction(manager, acting, targetedEntities);
                            foreach (var target in targetedEntities)
                            {
                                manager.RemoveComponentFromEntity<Targeted>(target);
                                target.Modulate = Colors.White;
                            }

                            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
                        },
                        Delay = 1.5f
                    });
                },
                Delay = 1.5f
            });
        }
        else
        {
            manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
            {
                Callback = () =>
                {
                    manager.AddComponentToEntity(manager.GetNewEntity(), new StatusTickEvent() { TickingEntity = acting });
                    manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
                }
            });
        }
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC done acting: " + acting.Name);
        if (targetLocations != null)
        {
            foreach (var spot in targetLocations)
            {
                manager.DeleteEntity(spot.Id);
            }
        }
    }
}