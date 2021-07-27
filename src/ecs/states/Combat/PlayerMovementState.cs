using Ecs;
using Godot;
using System.Collections.Generic;

public class PlayerMovementState : State
{
    private List<Entity> travelLocations = new List<Entity>();
    private Entity acting;
    private Map map;

    public PlayerMovementState(Entity acting, Entity map)
    {
        this.acting = acting;
        this.map = map.GetComponent<Map>();
    }

    public override void Pre(Manager manager)
    {
        manager.PerformHudAction("SetProfile", Direction.Left, acting);
        manager.PerformHudAction("SetProfile", Direction.Right, null);
        manager.PerformHudAction("SetActions", acting.GetComponentOrNull<MoveSet>());

        manager.AddComponentsToEntity(acting, new Selected());
        if (acting?.GetComponentOrNull<Movable>() != null)
        {
            MapUtils.RefreshObstacles(map, manager.GetEntitiesWithComponent<TileLocation>());

            var moveStats = acting.GetComponent<Movable>();

            // If we are /returning/ to this state by going backwards, then just use starting location instead of current
            var startingPosition = acting.GetComponent<TileLocation>().TilePosition;
            if (moveStats.StartingLocation != null)
            {
                startingPosition = moveStats.StartingLocation.TilePosition;
            }

            var points = map.AStar.GetPointsInRange(moveStats, startingPosition);
            // Add back our starting location so we can stay in place if desired
            points.Add(startingPosition);
            if (moveStats.StartingLocation != null)
            {
                points.Add(acting.GetComponent<TileLocation>().TilePosition);
            }
            else
            {
                acting.GetComponent<TurnSpeed>().TimeToAct = 20;
            }

            travelLocations = MapUtils.GenerateTileLocationsForPoints<TravelLocation>(manager, points, "res://img/tiles/image_part_029.png");
        }
    }

    public override void Post(Manager manager)
    {
        foreach (var spot in travelLocations)
        {
            manager.DeleteEntity(spot.Id);
        }

        manager.RemoveComponentFromEntity<Selected>(acting);
    }
}
