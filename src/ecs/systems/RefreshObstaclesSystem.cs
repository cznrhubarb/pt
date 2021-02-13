using Ecs;
using Godot;

public class RefreshObstaclesSystem : Ecs.System
{
    const string MapEntityKey = "map";

    public RefreshObstaclesSystem()
    {
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Map>(MapEntityKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
        map.AStar.ClearObstacles();

        base.UpdateAll(deltaTime);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
        var tilePosition = entity.GetComponent<TileLocation>().TilePosition;

        if (entity.HasComponent<PlayerCharacter>() || entity.HasComponent<FriendlyNpc>())
        {
            map.AStar.SetObstacle(tilePosition, Affiliation.Friendly);
        }
        else if (entity.HasComponent<EnemyNpc>())
        {
            map.AStar.SetObstacle(tilePosition, Affiliation.Enemy);
        }
        else if (entity.HasComponent<Obstacle>())
        {
            map.AStar.SetObstacle(tilePosition, Affiliation.Neutral);
        }
    }
}
