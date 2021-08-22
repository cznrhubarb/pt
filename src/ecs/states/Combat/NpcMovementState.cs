using Ecs;
using Godot;
using System.Collections.Generic;

public class NpcMovementState : State
{
    private List<Entity> travelLocations = new List<Entity>();
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

        List<Vector3> points;
        if (acting.GetComponent<StatusBag>().Statuses.ContainsKey("Immobilize"))
        {
            points = new List<Vector3>() { actorLocation.TilePosition };
        }
        else
        {
            var moveStats = acting.GetComponent<Movable>();
            var startingPosition = actorLocation.TilePosition;
            points = map.AStar.GetPointsInRange(moveStats, startingPosition);
        }
        travelLocations = MapUtils.GenerateTileLocationsForPoints<TravelLocation>(manager, points, "res://img/tiles/image_part_029.png");

        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () =>
            {
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
            },
            Delay = 1f
        });
    }

    public override void Post(Manager manager)
    {
        GD.Print("NPC done moving: " + acting.Name);

        // Maybe better before the move tween starts?
        foreach (var spot in travelLocations)
        {
            manager.DeleteEntity(spot.Id);
        }
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