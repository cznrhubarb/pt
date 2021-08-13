using Ecs;
using Godot;

public class NpcMovementState : State
{
    private Entity acting;
    private Map map;
    private TacticalPlan plan;

    public NpcMovementState(Entity acting, Map map, TacticalPlan plan)
    {
        this.acting = acting;
        this.map = map;
        this.plan = plan;
    }

    public override void Pre(Manager manager)
    {
        GD.Print("NPC moving: " + acting.Name);
        var actorLocation = acting.GetComponent<TileLocation>();

        // TODO: Display the potential move locations for the NPC for a second before actually moving

        if (actorLocation.TilePosition != plan.MoveTargetLocation)
        {
            var actorMovable = acting.GetComponent<Movable>();
            var path = map.AStar.GetPath(actorMovable, actorLocation.TilePosition, plan.MoveTargetLocation);
            acting.GetComponent<TurnSpeed>().TimeToAct += (path.Length - 1) * actorMovable.TravelSpeed;

            var tweenSeq = MapUtils.BuildTweenForActor(manager, acting, path);
            tweenSeq.Connect("finished", this, nameof(AdvanceState), new Godot.Collections.Array() { manager, 0.5f });
        }
        else
        {
            AdvanceState(manager, 0f);
        }
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC done moving: " + acting.Name);
    }

    private void AdvanceState(Manager manager, float delay)
    {
        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () => manager.ApplyState(new NpcTargetingState(acting, map, plan)),
            Delay = delay
        });
    }
}