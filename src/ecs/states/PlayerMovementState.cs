using Ecs;
using Godot;
using System.Collections.Generic;

public class PlayerTurnState : State
{
    private List<Entity> travelLocations = new List<Entity>();

    // TODO: These can be private, passed in via constructor
    public Entity Acting { get; set; }
    public Entity Map { get; set; }

    public override void Pre(Manager manager)
    {
        // TODO: Need another state for preview movement of entity
        // TODO: Move this code to a centralized location used by both states

        manager.AddComponentsToEntity(Acting, new Selected());
        if (Acting?.GetComponentOrNull<Movable>() != null)
        {
            var map = Map.GetComponent<Map>();
            var moveStats = Acting.GetComponent<Movable>();
            var tileLocation = Acting.GetComponent<TileLocation>();

            var points = map.AStar.GetPointsInRange(moveStats, tileLocation.TilePosition);
            // Add back our starting location so we can stay in place if desired
            points.Add(tileLocation.TilePosition);

            foreach (var point in points)
            {
                var spotEnt = manager.GetNewEntity();
                manager.AddComponentsToEntity(spotEnt,
                    new TileLocation() { TilePosition = new Vector3(point.x, point.y, point.z), ZLayer = 1 },
                    new SpriteWrap(),
                    new TravelLocation(),
                    new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 2.5f });

                Texture tex = null;
                if (Acting.HasComponent<PlayerCharacter>())
                {
                    tex = GD.Load<Texture>("res://img/tiles/image_part_029.png");
                }
                else if (Acting.HasComponent<FriendlyNpc>())
                {
                    tex = GD.Load<Texture>("res://img/tiles/image_part_031.png");
                }
                else if (Acting.HasComponent<EnemyNpc>())
                {
                    tex = GD.Load<Texture>("res://img/tiles/image_part_030.png");
                }
                var sprite = spotEnt.GetComponent<SpriteWrap>().Sprite;
                sprite.Modulate = new Color(1, 1, 1, 0.65f);
                sprite.Texture = tex;

                travelLocations.Add(spotEnt);
            }
        }
    }

    public override void Post(Manager manager)
    {
        var movableComp = Acting?.GetComponentOrNull<Movable>();
        if (movableComp != null)
        {
            movableComp.StartingLocation = null;
        }

        foreach (var spot in travelLocations)
        {
            manager.DeleteEntity(spot.Id);
        }

        manager.RemoveComponentFromEntity<Selected>(Acting);
    }
}
