using Ecs;
using Godot;
using System.Collections.Generic;

public class PlayerMovementState : State
{
    private List<Entity> travelLocations = new List<Entity>();

    // TODO: These can be private, passed in via constructor
    public Entity Acting { get; set; }
    public Entity Map { get; set; }

    public override void Pre(Manager manager)
    {
        // TODO: Need another state for preview movement of entity
        // TODO: Move this code to a centralized location used by both states
        (manager as Combat).SetProfile(Direction.Left, Acting);
        (manager as Combat).SetProfile(Direction.Right, null);
        (manager as Combat).SetActions(Acting.GetComponentOrNull<MoveSet>());

        manager.AddComponentsToEntity(Acting, new Selected());
        if (Acting?.GetComponentOrNull<Movable>() != null)
        {
            var map = Map.GetComponent<Map>();
            MapUtils.RefreshObstacles(map, manager.GetEntitiesWithComponent<TileLocation>());

            var moveStats = Acting.GetComponent<Movable>();

            // If we are /returning/ to this state by going backwards, then just use starting location instead of current
            var startingPosition = Acting.GetComponent<TileLocation>().TilePosition;
            if (moveStats.StartingLocation != null)
            {
                startingPosition = moveStats.StartingLocation.TilePosition;
            }

            var points = map.AStar.GetPointsInRange(moveStats, startingPosition);
            // Add back our starting location so we can stay in place if desired
            points.Add(startingPosition);
            if (moveStats.StartingLocation != null)
            {
                points.Add(Acting.GetComponent<TileLocation>().TilePosition);
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

        manager.RemoveComponentFromEntity<Selected>(Acting);
    }
}
