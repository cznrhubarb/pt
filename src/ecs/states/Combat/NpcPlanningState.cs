using Ecs;
using Godot;
using System.Linq;

public class NpcPlanningState : State
{
    private Entity acting;
    private Map map;

    public NpcPlanningState(Entity acting, Entity mapEnt)
    {
        this.acting = acting;
        this.map = mapEnt.GetComponent<Map>();
    }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC planning: " + acting.Name);
        manager.PerformHudAction("SetProfile", Direction.Left, acting);
        manager.PerformHudAction("SetProfile", Direction.Right, null);

        var statuses = acting.GetComponent<StatusBag>().Statuses;
        // TODO: Maybe "Captured" monsters have a chance to struggle free here?
        if (statuses.ContainsKey("Sleep") || statuses.ContainsKey("Captured"))
        {
            acting.GetComponent<TurnSpeed>().TimeToAct = 40;
            manager.AddComponentToEntity(manager.GetNewEntity(), new AdvanceClockEvent());
            return;
        }

        acting.GetComponent<TurnSpeed>().TimeToAct = 20;

        // Kinda dumb way to do this because we're not in a system
        var potentialTargets = manager.GetEntitiesWithComponent<ProfileDetails>()
            .Where(ent => ent.HasComponent<TileLocation>() && ent.HasComponent<FightStats>() && ent.HasComponent<Health>());

        MapUtils.RefreshObstacles(map, manager.GetEntitiesWithComponent<TileLocation>());
        var plan = Tactician.GetTurnPlan(acting, map, potentialTargets);

        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () => manager.ApplyState(new NpcMovementState(acting, map, plan))
        });
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC done planning: " + acting.Name);
    }
}