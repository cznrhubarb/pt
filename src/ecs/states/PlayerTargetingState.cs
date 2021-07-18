using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class PlayerTargetingState : State
{
    private List<Entity> targetLocations = new List<Entity>();
    private List<Entity> targetIndicators = new List<Entity>();
    private Entity acting;
    private Map map;

    public Move SelectedMove { get; private set; }

    public PlayerTargetingState(Entity acting, Entity map, Move selectedMove)
    {
        this.acting = acting;
        this.map = map.GetComponent<Map>();
        this.SelectedMove = selectedMove;
    }

    public override void Pre(Manager manager)
    {
        manager.AddComponentsToEntity(acting, new Selected());
        if (acting?.GetComponentOrNull<Movable>() != null)
        {
            var tilePosition = acting.GetComponent<TileLocation>().TilePosition;

            var points = map.AStar.GetPointsBetweenRange(tilePosition, SelectedMove.MinRange, SelectedMove.MaxRange)
                .Where(pt => tilePosition.z - pt.z <= SelectedMove.MaxHeightRangeDown && pt.z - tilePosition.z <= SelectedMove.MaxHeightRangeUp)
                .ToList();

            targetLocations = MapUtils.GenerateTileLocationsForPoints<TargetLocation>(manager, points, "res://img/tiles/image_part_031.png");

            // Max number of indicators we need is defined by [x = maxRange, y = minRange, n = numNeeded]
            //  n = x * (x + 1) * 2 - y * (y - 1) * 2 (+1 if y = 0)
            var minAreaOfEffect = 0;
            var maxPoints = SelectedMove.AreaOfEffect * (SelectedMove.AreaOfEffect + 1) * 2 
                            - minAreaOfEffect * (minAreaOfEffect - 1) * 2 
                            + 1;
            points = Enumerable.Range(1, maxPoints).Select(_i => new Vector3(tilePosition)).ToList();
            targetIndicators = MapUtils.GenerateTileLocationsForPoints<TargetIndicator>(manager, points, "res://img/tiles/image_part_030.png");
            foreach (var indicator in targetIndicators)
            {
                indicator.GetComponent<SpriteWrap>().Sprite.Visible = false;
            }

            var turnSpeed = acting.GetComponent<TurnSpeed>();
            turnSpeed.TimeToAct += SelectedMove.Speed;
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

        manager.RemoveComponentFromEntity<Selected>(acting);
    }
}
