using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AStarEx : AStar
{
    // AStarEx is tied to TileMap right now. Is this necessarily a bad thing?
    //  Pros are that it is done already, and it will always be used with a tile map
    //  Cons are that testing is much harder, and maybe I want to make my tiles have 
    //      additional information in the future that tilemap doesn't store, like terrain types

    private const int MinimumHeadRoom = 5;
    private const int MaxWidth = 100;
    private const int MaxLayers = 100;
    private const int MaxHeight = 30;

    private struct MapSortPoint
    {
        public MapSortPoint(int p, float c) { point = p; remainingCost = c; }
        public int point;
        public float remainingCost;
    }

    // TODO: second parameter should be a TerrainType enum
    //private Dictionary<int, int> terrainTypes;

    private Movable mover;

    public AStarEx(List<TileMap> tileMaps)
    {
        Build(tileMaps);
    }

    public override float _ComputeCost(int fromId, int toId)
    {
        return Mathf.Sqrt(_EstimateCost(fromId, toId));
    }

    public override float _EstimateCost(int fromId, int toId)
    {
        var fromPos = GetPointPosition(fromId);
        var toPos = GetPointPosition(toId);

        if (Mathf.Abs(fromPos.z - toPos.z) <= mover.MaxJump)
        {
            // TODO: Take into account TerrainType vs moveStats affinities
            return Mathf.Pow(fromPos.x - toPos.x, 2) + Mathf.Pow(fromPos.y - toPos.y, 2);
        }
        else
        {
            return float.MaxValue;
        }
    }

    private int HashId(Vector3 position) =>
        Convert.ToInt32(position.x + position.y * MaxWidth + position.z * MaxWidth * MaxLayers) + MaxWidth * MaxLayers * MaxHeight;

    private string FlatKey(Vector2 position) =>
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

    private void Build(List<TileMap> tileMaps)
    {
        // TODO: MAP
        var flatMap = new Dictionary<string, List<int>>();

        for (var height = tileMaps.Count - 1; height >= 0; height--)
        {
            var map = tileMaps[height];
            foreach (Vector2 tilePos in map.GetUsedCells())
            {
                var position = new Vector3(tilePos.x, tilePos.y, height);
                // Assumes that we're going top to bottom. If we switch to a straight list,
                //  we'll need to switch this to doing this in the second pass through and removing
                //  Or a set by z before we go in here
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
                var hash = HashId(new Vector3(tilePos.x, tilePos.y, height));
                if (!HasPoint(hash))
                {
                    continue;
                }

                foreach (var offset in Offsets)
                {
                    var flatKey = FlatKey(tilePos + offset);
                    if (flatMap.ContainsKey(flatKey))
                    {
                        foreach (var tileId in flatMap[flatKey])
                        {
                            ConnectPoints(hash, tileId);
                        }
                    }
                }
            }
        }
    }

    public Vector3[] GetPath(Movable moveStats, int startingPoint, int endingPoint)
    {
        mover = moveStats;
        return GetPointPath(startingPoint, endingPoint);
    }

    public List<Vector3> GetPointsInRange(Movable moveStats, int startingPoint)
    {
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
        return pointsInRange.Select(msp => GetPointPosition(msp.point)).ToList();
    }
}
