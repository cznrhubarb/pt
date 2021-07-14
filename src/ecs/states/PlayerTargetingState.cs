using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class PlayerTargetingState : State
{
    private List<Entity> targetLocations = new List<Entity>();
    private List<Entity> targetIndicators = new List<Entity>();

    // TODO: These can be private, passed in via constructor
    public Entity Acting { get; set; }
    public Entity Map { get; set; }
    public Move SelectedMove { get; set; }

    public override void Pre(Manager manager)
    {
        manager.AddComponentsToEntity(Acting, new Selected());
        if (Acting?.GetComponentOrNull<Movable>() != null)
        {
            var map = Map.GetComponent<Map>();
            var tilePosition = Acting.GetComponent<TileLocation>().TilePosition;

            var points = map.AStar.GetPointsBetweenRange(tilePosition, SelectedMove.MinRange, SelectedMove.MaxRange)
                .Where(pt => tilePosition.z - pt.z <= SelectedMove.MaxHeightRangeDown && pt.z - tilePosition.z <= SelectedMove.MaxHeightRangeUp)
                .ToList();

            targetLocations = MapUtils.GenerateTileLocationsForPoints<TargetLocation>(manager, points, "res://img/tiles/image_part_031.png");

            // TODO: Is this supposed to be AOE range instead of move range?
            // Max number of indicators we need is defined by [x = maxRange, y = minRange, n = numNeeded]
            //  n = x * (x + 1) * 2 - y * (y - 1) * 2 (+1 if y = 0)
            var maxPoints = SelectedMove.MaxRange * (SelectedMove.MaxRange + 1) * 2 
                            - SelectedMove.MinRange * (SelectedMove.MinRange - 1) * 2 
                            + 1;
            points = Enumerable.Range(1, maxPoints).Select(_i => new Vector3(tilePosition)).ToList();
            targetIndicators = MapUtils.GenerateTileLocationsForPoints<TargetIndicator>(manager, points, "res://img/tiles/image_part_030.png");
            foreach (var indicator in targetIndicators)
            {
                indicator.GetComponent<SpriteWrap>().Sprite.Visible = false;
            }
        }
    }

    public override void Post(Manager manager)
    {
        foreach (var spot in targetLocations)
        {
            manager.DeleteEntity(spot.Id);
        }
        foreach (var spot in targetIndicators)
        {
            manager.DeleteEntity(spot.Id);
        }

        manager.RemoveComponentFromEntity<Selected>(Acting);
    }
}
