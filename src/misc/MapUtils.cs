
using Ecs;
using Godot;
using System.Collections.Generic;

public class MapUtils
{
    public static List<Entity> GenerateTravelLocationsForPoints(Manager manager, List<Vector3> points, string texturePath)
    {
        var travelLocations = new List<Entity>();

        foreach (var point in points)
        {
            var spotEnt = manager.GetNewEntity();
            manager.AddComponentsToEntity(spotEnt,
                new TileLocation() { TilePosition = new Vector3(point.x, point.y, point.z), ZLayer = 1 },
                new SpriteWrap(),
                new TravelLocation(),
                new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 2.5f });

            var sprite = spotEnt.GetComponent<SpriteWrap>().Sprite;
            sprite.Modulate = new Color(1, 1, 1, 0.65f);
            sprite.Texture = GD.Load<Texture>(texturePath);

            travelLocations.Add(spotEnt);
        }

        return travelLocations;
    }

    public static void RefreshObstacles(Map mapComponent, List<Entity> entities)
    {
        mapComponent.AStar.ClearObstacles();

        foreach (var entity in entities)
        {
            var tilePosition = entity.GetComponent<TileLocation>().TilePosition;

            if (entity.HasComponent<PlayerCharacter>() || entity.HasComponent<FriendlyNpc>())
            {
                mapComponent.AStar.SetObstacle(tilePosition, Affiliation.Friendly);
            }
            else if (entity.HasComponent<EnemyNpc>())
            {
                mapComponent.AStar.SetObstacle(tilePosition, Affiliation.Enemy);
            }
            else if (entity.HasComponent<Obstacle>())
            {
                mapComponent.AStar.SetObstacle(tilePosition, Affiliation.Neutral);
            }
        }
    }
}