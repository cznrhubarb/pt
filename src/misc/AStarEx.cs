using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AStarEx : AStar
{
    private const float ArbitraryLargeValue = 999999;
    private const int MinimumHeadRoom = 5;
    private const int MaxWidth = 100;
    private const int MaxLayers = 100;
    private const int MaxHeight = 30;

    private Dictionary<TerrainType, float> DefaultCosts = new Dictionary<TerrainType, float>()
    {
        { TerrainType.Even, 1 },
        { TerrainType.Difficult, 1 },
        { TerrainType.Stone, 1 },
        { TerrainType.Wood, 1 },
        { TerrainType.Grass, 1 },
        { TerrainType.Dirt, 1 },
        { TerrainType.Water, 2 },
        { TerrainType.DeepWater, 99 },
        { TerrainType.Sand, 1 },
    };
    //private Dictionary<TerrainType, float> DefaultCosts = new Dictionary<TerrainType, float>()
    //{
    //    { TerrainType.Even, 1 },
    //    { TerrainType.Difficult, 2 },
    //    { TerrainType.Stone, 1 },
    //    { TerrainType.Wood, 1 },
    //    { TerrainType.Grass, 1.2f },
    //    { TerrainType.Dirt, 1.1f },
    //    { TerrainType.Water, 2 },
    //    { TerrainType.DeepWater, 99 },
    //    { TerrainType.Sand, 1.4f },
    //};

    private struct MapSortPoint
    {
        public MapSortPoint(int p, float c) { point = p; remainingCost = c; }
        public int point;
        public float remainingCost;
    }

    private Movable mover = null;
    private Dictionary<int, TerrainType> terrainTypes = new Dictionary<int, TerrainType>();
    private Dictionary<int, Affiliation> obstacles = new Dictionary<int, Affiliation>();

    public AStarEx(IsoMap map)
    {
        Build(map);
    }

    public override float _ComputeCost(int fromId, int toId)
    {
        var fromPos = GetPointPosition(fromId);
        var toPos = GetPointPosition(toId);

        var differingObstacleAtLocation =
            mover != null &&
            obstacles.ContainsKey(toId) &&
            (obstacles[toId] == Affiliation.Neutral || obstacles[toId] != mover.Affiliation);

        var baseCost = Mathf.Pow(fromPos.x - toPos.x, 2) + Mathf.Pow(fromPos.y - toPos.y, 2);
        var terrain = terrainTypes[toId];
        var costModifier =
            mover == null
                ? 1
                : mover.TerrainCostModifiers.ContainsKey(terrain)
                    ? mover.TerrainCostModifiers[terrain]
                    : DefaultCosts[terrain];
        costModifier = Mathf.Pow(costModifier, 2);

        // TODO: Not sure if there is a better way to block off these paths?
        //  Maybe some use of DisconnectPoints/ConnectPoints to block off height deltas
        //  and SetPointDisabled to block off obstacles
        //  And do all of that in the one calling method (when mover is set), and then I don't need to keep mover as a local variable
        if (Mathf.Abs(fromPos.z - toPos.z) > (mover?.MaxJump ?? 99) || differingObstacleAtLocation)
        {
            baseCost += ArbitraryLargeValue;
        }

        return Mathf.Sqrt(baseCost * costModifier);
    }

    public override float _EstimateCost(int fromId, int toId)
    {
        // No savings to be had (or at least I don't know what is safe to consider a saving)
        // TODO: Might be able ot optimize here by moving estimate cost into calculate and then
        return _ComputeCost(fromId, toId);
    }

    private int HashId(Vector3 position) =>
        Convert.ToInt32(position.x + position.y * MaxWidth + position.z * MaxWidth * MaxLayers) + MaxWidth * MaxLayers * MaxHeight;

    private string FlatKey(Vector3 position) =>
        position.x + "," + position.y;

    private bool AnyTileInHeadSpace(Vector3 position)
    {
        var testPosition = new Vector3(position);
        for (var heightDelta = 1; heightDelta < MinimumHeadRoom; heightDelta++)
        {
            testPosition.z = position.z + heightDelta;
            if (HasPoint(HashId(testPosition)))
            {
                return true;
            }
        }

        return false;
    }

    private void Build(IsoMap map)
    {
        var flatMap = new Dictionary<string, List<int>>();

        var sortedTiles = map.Tiles
            .Select(ent => new TileData() 
            { 
                position = ent.GetComponent<TileLocation>().TilePosition, 
                terrain = ent.GetComponent<Terrain>().Type
            })
            .OrderBy(t => -t.position.z);

        foreach (var tile in sortedTiles)
        {
            if (!AnyTileInHeadSpace(tile.position))
            {
                var hash = HashId(tile.position);
                AddPoint(hash, tile.position);
                terrainTypes[hash] = tile.terrain;

                var flatKey = FlatKey(tile.position);
                if (!flatMap.ContainsKey(flatKey))
                {
                    flatMap.Add(flatKey, new List<int>());
                }
                flatMap[flatKey].Add(hash);
            }
        }

        var Offsets = new List<Vector3>() { new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 1, 0) };
        foreach (int point in GetPoints())
        {
            var pointPos = GetPointPosition(point);
            foreach (var offset in Offsets)
            {
                var flatKey = FlatKey(pointPos + offset);
                if (flatMap.ContainsKey(flatKey))
                {
                    foreach (var tileId in flatMap[flatKey])
                    {
                        ConnectPoints(point, tileId);
                    }
                }
            }
        }
    }

    public Vector3[] GetPath(Movable moveStats, Vector3 startPosition, Vector3 endPosition)
    {
        mover = moveStats;

        // GetPoints / GetPointConnections

        return GetPointPath(GetClosestPoint(startPosition), GetClosestPoint(endPosition));
    }

    public List<Vector3> GetPointsInRange(Movable moveStats, Vector3 startPosition)
    {
        var startingPoint = GetClosestPoint(startPosition);
        var pointsInRange = new List<MapSortPoint>() { new MapSortPoint(startingPoint, moveStats.MaxMove) };

        mover = moveStats;

        var evalIndex = 0;
        while (evalIndex < pointsInRange.Count)
        {
            var from = pointsInRange[evalIndex];
            if (Mathf.IsZeroApprox(from.remainingCost))
            {
                break;
            }

            var connections = GetPointConnections(from.point);

            foreach (var connection in connections)
            {
                if (!pointsInRange.Any(msp => msp.point == connection))
                {
                    var cost = _ComputeCost(from.point, connection);
                    var newCostAtDest = from.remainingCost - cost;
                    if (newCostAtDest >= 0)
                    {
                        var insertIdx = pointsInRange.FindIndex(msp => newCostAtDest > msp.remainingCost);
                        insertIdx = insertIdx != -1 ? insertIdx : pointsInRange.Count;
                        pointsInRange.Insert(insertIdx, new MapSortPoint(connection, newCostAtDest));
                    }
                }
            }

            evalIndex++;
        }

        var retPoints = pointsInRange
            .Where(msp => !obstacles.ContainsKey(msp.point))
            .Select(msp => GetPointPosition(msp.point))
            .ToList();
        // Re-add start position if it gets dropped as an obstacle
        if (!retPoints.Contains(startPosition))
        {
            retPoints.Add(startPosition);
        }
        return retPoints;
    }

    public List<Vector3> GetPointsBetweenRange(Vector3 startPosition, int minRange, int maxRange)
    {
        var results = new List<Vector3>();

        var startingPoint = GetClosestPoint(startPosition);
        var pointsInRange = new List<MapSortPoint>() { new MapSortPoint(startingPoint, maxRange) };

        mover = null;

        var evalIndex = 0;
        while (evalIndex < pointsInRange.Count)
        {
            var from = pointsInRange[evalIndex];
            if (Mathf.IsZeroApprox(from.remainingCost))
            {
                break;
            }

            var connections = GetPointConnections(from.point);

            foreach (var connection in connections)
            {
                if (!pointsInRange.Any(msp => msp.point == connection))
                {
                    var cost = _ComputeCost(from.point, connection);
                    var newCostAtDest = from.remainingCost - cost;
                    if (newCostAtDest >= 0)
                    {
                        var insertIdx = pointsInRange.FindIndex(msp => newCostAtDest > msp.remainingCost);
                        insertIdx = insertIdx != -1 ? insertIdx : pointsInRange.Count;
                        pointsInRange.Insert(insertIdx, new MapSortPoint(connection, newCostAtDest));
                    }
                }
            }

            evalIndex++;
        }

        return pointsInRange
            .Where(msp => (maxRange - msp.remainingCost) >= minRange)
            .Select(msp => GetPointPosition(msp.point))
            .ToList();
    }

    public void SetObstacle(Vector3 position, Affiliation affiliation)
    {
        obstacles.Add(HashId(position), affiliation);
    }

    public void ClearObstacles()
    {
        obstacles.Clear();
    }

    public Vector3? GetBestTileMatch(Vector3 position, int maxJump) =>
        GetBestTileMatch(position, maxJump, new HashSet<TerrainType>());

    public Vector3? GetBestTileMatch(Vector3 position, int maxJump, HashSet<TerrainType> impassableTerrains)
    {
        bool isViableTerrain(int hId) => !obstacles.ContainsKey(hId) && !impassableTerrains.Contains(terrainTypes[hId]);

        var hashId = HashId(position);
        if (HasPoint(hashId))
        {
            if (isViableTerrain(hashId))
            {
                return position;
            }
            else
            {
                return null;
            }
        }
        else 
        {
            var direction = AnyTileInHeadSpace(position) ? Vector3.Back : Vector3.Forward;
            var searchPos = position;
            var jumpDifference = 0;
            do
            {
                searchPos += direction;
                hashId = HashId(searchPos);
                jumpDifference++;
            } while (!HasPoint(hashId) && jumpDifference < maxJump);

            if (HasPoint(hashId) && isViableTerrain(hashId))
            {
                return searchPos;
            }
            else
            {
                return null;
            }
        }
    }

    private struct TileData
    {
        public Vector3 position;
        public TerrainType terrain;
    }
}
