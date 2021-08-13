using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class NpcTargetingState : State
{
    private Entity acting;
    private Map map;
    private TacticalPlan plan;

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
            plan.SelectedSkill.CurrentTP--;
            
            var skillTargetPoints = TargetUtils.GetTargetLocations(plan.SelectedSkill, map, plan.SkillTargetLocation);

            targetLocations = MapUtils.GenerateTileLocationsForPoints<TargetLocation>(manager, skillTargetPoints, "res://img/tiles/image_part_031.png");

            // TODO: If we put the targeteds in the tactical plan somehow, we can avoid doing a lot of the stuff in here
            // Kinda dumb way to do this because we're not in a system
            var potentialTargets = manager.GetEntitiesWithComponent<ProfileDetails>()
                .Where(ent => ent.HasComponent<TileLocation>() && ent.HasComponent<FightStats>() && ent.HasComponent<Health>());

            var actualTargets = TargetUtils.MarkTargets(manager, plan.SelectedSkill, acting, potentialTargets, skillTargetPoints);

            // TODO: Need to somehow highlight what the selected skill is that the NPC is using

            manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
            {
                Callback = () =>
                {
                    acting.GetComponent<TurnSpeed>().TimeToAct += plan.SelectedSkill.Speed;
                    TargetUtils.PerformAction(manager, actualTargets);
                    manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
                },
                Delay = 1.5f
            });
        }
        else
        {
            manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
            {
                Callback = () => manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent())
            });
        }
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC done acting: " + acting.Name);
        foreach (var spot in targetLocations)
        {
            manager.DeleteEntity(spot.Id);
        }
    }
}