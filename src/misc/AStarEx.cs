using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AStarEx : AStar
{
    private const int MinimumHeadRoom = 5;
    private const int MaxWidth = 100;
    private const int MaxLayers = 100;

    // TODO: second parameter should be a TerrainType enum
    private Dictionary<int, int> terrainTypes;

    private MoveStats mover;

    public override float _ComputeCost(int fromId, int toId)
    {
        return Mathf.Sqrt(_EstimateCost(fromId, toId));
    }

    public override float _EstimateCost(int fromId, int toId)
    {
        var fromPos = GetPointPosition(fromId);
        var toPos = GetPointPosition(toId);

        if (Mathf.Abs(fromPos.z - toPos.z) <= mover.Jump)
        {
            // TODO: Take into account TerrainType vs moveStats affinities
            return (fromPos.x - toPos.x) * (fromPos.y - toPos.y);
        }
        else
        {
            return float.MaxValue;
        }
    }

    private int HashId(Vector3 position) =>
        Convert.ToInt32(position.x + position.y * MaxWidth + position.z * MaxWidth * MaxLayers);

    private string FlatKey(Vector2 position) =>
        position.x + "," + position.y;

    private bool AnyTileInHeadSpace(Vector3 position)
    {
        var testPosition = new Vector3(position);
        for (var heightDelta = 1; heightDelta < MinimumHeadRoom; heightDelta++)
        {
            testPosition.z = position.z + heightDelta * MaxWidth * MaxLayers;
            if (HasPoint(HashId(testPosition)))
            {
                return true;
            }
        }

        return false;
    }

    public void Build(List<TileMap> tileMaps)
    {
        var flatMap = new Dictionary<string, List<int>>();

        for (var height = tileMaps.Count - 1; height >= 0; height--)
        {
            var map = tileMaps[height];
            foreach (Vector2 tilePos in map.GetUsedCells())
            {
                var position = new Vector3(tilePos.x, tilePos.y, height);
                if (!AnyTileInHeadSpace(position))
                {
                    var hash = HashId(position);
                    AddPoint(hash, position);
                    // TODO: Add it to the terrain type map also

                    var flatKey = FlatKey(tilePos);
                    if (!flatMap.ContainsKey(flatKey))
                    {
                        flatMap.Add(flatKey, new List<int>());
                    }
                    flatMap[flatKey].Add(hash);
                }
            }
        }

        var Offsets = new List<Vector2>() { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) };
        for (var height = 0; height < tileMaps.Count; height++)
        {
            var map = tileMaps[height];
            foreach (Vector2 tilePos in map.GetUsedCells())
            {
                foreach (var offset in Offsets)
                {
                    var flatKey = FlatKey(tilePos + offset);
                    if (flatMap.ContainsKey(flatKey))
                    {
                        foreach (var tileId in flatMap[flatKey])
                        {
                            ConnectPoints(HashId(new Vector3(tilePos.x, tilePos.y, height)), tileId);
                        }
                    }
                }
            }
        }
    }

    public Vector3[] GetPath(MoveStats moveStats, int startingPoint, int endingPoint)
    {
        mover = moveStats;
        return GetPointPath(startingPoint, endingPoint);
    }

    public List<Vector3> GetPointsInRange(MoveStats moveStats, int startingPoint, float maxCost)
    {
        var pointsInRange = new List<int>() { startingPoint };

        mover = moveStats;
        rGetPointsInRange(startingPoint, pointsInRange, maxCost);

        pointsInRange.RemoveAt(0);
        return pointsInRange.Select(pointId => GetPointPosition(pointId)).ToList();
    }

    private void rGetPointsInRange(int fromPoint, List<int> pointsInRange, float remainingCost)
    {
        var connections = GetPointConnections(fromPoint);

        foreach (var connection in connections)
        {
            if (!pointsInRange.Contains(connection))
            {
                var cost = _ComputeCost(fromPoint, connection);
                if (cost <= remainingCost)
                {
                    pointsInRange.Add(connection);
                    rGetPointsInRange(connection, pointsInRange, remainingCost - cost);
                }
            }
        }
    }
}
