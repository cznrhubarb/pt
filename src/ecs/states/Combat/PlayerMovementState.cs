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
        manager.PerformHudAction("SetActions", acting.GetComponentOrNull<SkillSet>(), acting.GetComponent<StatusBag>().Statuses.ContainsKey("Silence"));

        manager.AddComponentsToEntity(acting, new Selected());
        if (acting?.GetComponentOrNull<Movable>() != null)
        {
            MapUtils.RefreshObstacles(map, manager.GetEntitiesWithComponent<TileLocation>());

            List<Vector3> points;
            if (acting.GetComponent<StatusBag>().Statuses.ContainsKey("Immobilize"))
            {
                points = new List<Vector3>() { acting.GetComponent<TileLocation>().TilePosition };
            }
            else
            {
                var moveStats = acting.GetComponent<Movable>();
                var affiliation = acting.GetComponent<Affiliated>().Affiliation;

                // If we are /returning/ to this state by going backwards, then just use starting location instead of current
                var startingPosition = acting.GetComponent<TileLocation>().TilePosition;
                if (moveStats.StartingLocation != null)
                {
                    startingPosition = moveStats.StartingLocation.TilePosition;
                }

                points = map.AStar.GetPointsInRange(moveStats, affiliation, startingPosition);
                if (moveStats.StartingLocation != null)
                {
                    points.Add(acting.GetComponent<TileLocation>().TilePosition);
                }
                else
                {
                    acting.GetComponent<TurnSpeed>().TimeToAct = 20;
                }
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
