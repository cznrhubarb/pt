using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AStarEx : AStar
{
    private const int MinimumHeadRoom = 5;
    private const int MaxWidth = 100;
    private const int MaxLayers = 100;
    private const int MaxHeight = 30;

    private Dictionary<TerrainType, float> DefaultCosts = new Dictionary<TerrainType, float>()
    {
        { TerrainType.Even, 1 },
        { TerrainType.Difficult, 2 },
        { TerrainType.Stone, 1 },
        { TerrainType.Wood, 1 },
        { TerrainType.Grass, 1.2f },
        { TerrainType.Dirt, 1.1f },
        { TerrainType.Water, 2 },
        { TerrainType.DeepWater, 99 },
        { TerrainType.Sand, 1.4f },
    };

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
        return Mathf.Sqrt(_EstimateCost(fromId, toId));
    }

    public override float _EstimateCost(int fromId, int toId)
    {
        var fromPos = GetPointPosition(fromId);
        var toPos = GetPointPosition(toId);

        var differingObstacleAtLocation = obstacles.ContainsKey(toId)
            ? obstacles[toId] != mover.Affiliation
            : false;

        if (Mathf.Abs(fromPos.z - toPos.z) <= mover.MaxJump && !differingObstacleAtLocation)
        {
            var baseCost = Mathf.Pow(fromPos.x - toPos.x, 2) + Mathf.Pow(fromPos.y - toPos.y, 2);
            var terrain = terrainTypes[toId];
            var costModifier = mover.TerrainCostModifiers.ContainsKey(terrain)
                ? mover.TerrainCostModifiers[terrain]
                : DefaultCosts[terrain];
            costModifier = Mathf.Pow(costModifier, 2);
            return baseCost * costModifier;
        }
        else
        {
            return float.MaxValue;
        }
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

        pointsInRange.RemoveAt(0);
        return pointsInRange
            .Where(msp => !obstacles.ContainsKey(msp.point))
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

    private struct TileData
    {
        public Vector3 position;
        public TerrainType terrain;
    }
}
