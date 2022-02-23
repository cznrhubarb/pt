
using Ecs;
using Godot;
using System.Collections.Generic;

public class MapUtils
{
    public static List<Entity> GenerateTileLocationsForPoints<T>(Manager manager, List<Vector3> points, string texturePath) where T : Component, new()
    {
        var tileLocations = new List<Entity>();

        foreach (var point in points)
        {
            var spotEnt = manager.GetNewEntity();
            manager.AddComponentsToEntity(spotEnt,
                new TileLocation() { TilePosition = new Vector3(point.x, point.y, point.z), ZLayer = 1 },
                new SpriteWrap(),
                new T(),
                new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 2.5f });

            var sprite = spotEnt.GetComponent<SpriteWrap>().Sprite;
            sprite.Modulate = new Color(1, 1, 1, 0.65f);
            sprite.Texture = GD.Load<Texture>(texturePath);

            tileLocations.Add(spotEnt);
        }

        return tileLocations;
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

    public static TweenSequence BuildTweenForActor(Manager manager, Entity movingActor, Vector3[] path)
    {
        var tweenSeq = new TweenSequence(manager.GetTree());
        for (var idx = 1; idx < path.Length; idx++)
        {
            tweenSeq.AppendCallback(movingActor, "SetDirection", new object[] { (path[idx] - path[idx - 1]).ToDirection() });
            // TODO: Slightly roundabout way of tweening our actors, primarily for the benefit of z sorting, and it still isn't perfect :(
            if (path[idx].z != path[idx - 1].z)
            {
                // Jump
                var ease = path[idx].z > path[idx - 1].z ? Tween.EaseType.Out : Tween.EaseType.In;
                tweenSeq.AppendInterval(0.1f);
                tweenSeq.AppendMethod(movingActor, "SetTilePositionXY", path[idx - 1], path[idx], 0.2f);
                tweenSeq.Join();
                tweenSeq.AppendMethod(movingActor, "SetTilePositionZ", path[idx - 1].z, path[idx].z, 0.2f)
                    .SetTransition(Tween.TransitionType.Back)
                    .SetEase(ease);
                tweenSeq.AppendInterval(0.1f);
            }
            else
            {
                // Walk
                tweenSeq.AppendMethod(movingActor, "SetTilePositionXY", path[idx - 1], path[idx], 0.2f);
            }
        }
        manager.AddComponentToEntity(movingActor, new Tweening() { TweenSequence = tweenSeq });

        return tweenSeq;
    }
}