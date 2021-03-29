using Ecs;
using Godot;
using System;
using System.Collections.Generic;

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
            var tileLocation = Acting.GetComponent<TileLocation>();

            var points = map.AStar.GetPointsBetweenRange(tileLocation.TilePosition, SelectedMove.MinRange, SelectedMove.MaxRange);

            targetLocations = MapUtils.GenerateTravelLocationsForPoints(manager, points, "res://img/tiles/image_part_031.png");

            points = map.AStar.GetPointsBetweenRange(tileLocation.TilePosition, 0, SelectedMove.AreaOfEffect);
            targetIndicators = MapUtils.GenerateTravelLocationsForPoints(manager, points, "res://img/tiles/image_part_030.png");
            foreach (var indicator in targetIndicators)
            {
                manager.RemoveComponentFromEntity<TravelLocation>(indicator);
                manager.AddComponentToEntity(indicator, new TargetIndicator());
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
