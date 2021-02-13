using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class AStarExTest : WAT.Test
{
    private Movable TestMover 
    { 
        get => new Movable() 
        { 
            MaxMove = 2, 
            MaxJump = 1, 
            StartingLocation = new TileLocation() { TilePosition = new Vector3(0, 0, 0) }, 
            Affiliation = Affiliation.Friendly 
        };
    }

    public override string Title()
    {
        return "AStarEx";
    }

    [Test]
    public void Constructor_SetsPoints()
    {
        var map = GenerateMap(new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(1, 0, 0) });
        var astar = new AStarEx(map);

        var firstPoint = astar.GetClosestPoint(new Vector3(0, 0, 0));
        var secondPoint = astar.GetClosestPoint(new Vector3(1, 0, 0));
        Assert.IsNotEqual(firstPoint, -1);
        Assert.IsNotEqual(secondPoint, -1);
        Assert.IsNotEqual(firstPoint, secondPoint);
    }

    [Test]
    public void GetPointsInRange_ReturnsNoPoints_StandingOnOnlyTile()
    {
        var map = GenerateMap(new List<Vector3>() { new Vector3(0, 0, 0) });
        var astar = new AStarEx(map);

        Assert.IsEqual(astar.GetPointsInRange(TestMover, new Vector3(0, 0, 0)).Count, 0);
    }

    [Test]
    public void GetPointsInRange_ReturnsPoints_SurroundedByPoints()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, -1, 0)
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        var results = astar.GetPointsInRange(TestMover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 4);
        for (var i = 1; i < points.Count; i++)
        {
            Assert.ListContains(results, points[i]);
        }
    }

    [Test]
    public void GetPointsInRange_DoesNotReturnPointsOutsideRange_PlentyOfPoints()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, -1, 0),
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, -1, 0),
            new Vector3(2, 0, 0),
            new Vector3(0, 2, 0),
            new Vector3(-2, 0, 0),
            new Vector3(0, -2, 0),
            new Vector3(1, -2, 0)   // Too far
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        var results = astar.GetPointsInRange(TestMover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 12);
        for (var i = 1; i < points.Count - 1; i++)
        {
            Assert.ListContains(results, points[i]);
        }
    }

    [Test]
    public void GetPointsInRange_ReturnsNoPoints_BigHeightDifference()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 2),
            new Vector3(0, 1, 2),
            new Vector3(-1, 0, 2),
            new Vector3(0, -1, 2)
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        var results = astar.GetPointsInRange(TestMover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 0);
    }

    [Test]
    public void GetPointsInRange_ReturnsReachablePoints_HeightObstacles()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 2),
            new Vector3(1, -1, 2),
            new Vector3(0, 1, 2),   // Too far
            new Vector3(-1, 0, 2),  // Can't jump
            new Vector3(0, -1, 2)   // Too far
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        var results = astar.GetPointsInRange(TestMover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 3);
        for (var i = 1; i <= 3; i++)
        {
            Assert.ListContains(results, points[i]);
        }
    }

    [Test]
    public void GetPointsInRange_ReturnsReachablePoints_DifferingAffiliationObstacle()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(3, 0, 0),   // Blocked
            new Vector3(4, 0, 0),
            new Vector3(5, 0, 0),
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        astar.SetObstacle(new Vector3(3, 0, 0), Affiliation.Enemy);

        var mover = TestMover;
        mover.MaxMove = 5;

        var results = astar.GetPointsInRange(mover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 2);
        for (var i = 1; i <= 2; i++)
        {
            Assert.ListContains(results, points[i]);
        }
    }

    [Test]
    public void GetPointsInRange_ReturnsPointsOnBothSides_MatchingAffiliationObstacles()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(3, 0, 0),   // Friendly
            new Vector3(4, 0, 0),
            new Vector3(5, 0, 0),
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        astar.SetObstacle(new Vector3(3, 0, 0), Affiliation.Friendly);

        var mover = TestMover;
        mover.MaxMove = 5;

        var results = astar.GetPointsInRange(mover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 4);
        Assert.ListContains(results, points[1]);
        Assert.ListContains(results, points[2]);
        Assert.ListDoesNotContain(results, points[3]);
        Assert.ListContains(results, points[4]);
        Assert.ListContains(results, points[5]);
    }

    [Test]
    public void GetPointsInRange_ReturnsReachablePoints_DifficultTerrain()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),   // Even
            new Vector3(2, 0, 0),   // Difficult
            new Vector3(3, 0, 0),   // Difficult
            new Vector3(4, 0, 0),   // Difficult
            new Vector3(5, 0, 0),   // Difficult
        };
        var terrains = new List<TerrainType>()
        {
            TerrainType.Even,
            TerrainType.Even,
            TerrainType.Difficult,
            TerrainType.Difficult,
            TerrainType.Difficult,
            TerrainType.Difficult,
        };
        var map = GenerateMapWithTerrain(points, terrains);
        var astar = new AStarEx(map);

        var mover = TestMover;
        mover.MaxMove = 5;

        var results = astar.GetPointsInRange(mover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 3);
    }

    [Test]
    public void GetPointsInRange_UsesMoverTerrainCosts_TerrainCostOverrideExists()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),   // Even
            new Vector3(2, 0, 0),   // Difficult
            new Vector3(3, 0, 0),   // Difficult
            new Vector3(4, 0, 0),   // Difficult
            new Vector3(5, 0, 0),   // Difficult
        };
        var terrains = new List<TerrainType>()
        {
            TerrainType.Even,
            TerrainType.Even,
            TerrainType.Difficult,
            TerrainType.Difficult,
            TerrainType.Difficult,
            TerrainType.Difficult,
        };
        var map = GenerateMapWithTerrain(points, terrains);
        var astar = new AStarEx(map);

        var mover = TestMover;
        mover.MaxMove = 2;
        mover.TerrainCostModifiers.Add(TerrainType.Difficult, 0.5f);

        var results = astar.GetPointsInRange(mover, new Vector3(0, 0, 0));
        Assert.IsEqual(results.Count, 3);
    }

    [Test]
    public void GetPath_ReturnsShortestPathRespectingObstacles_AvailablePath()
    {
        var points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 2),
            new Vector3(1, -1, 2),
            new Vector3(0, 1, 2),   // Too far
            new Vector3(-1, 0, 2),  // Can't jump
            new Vector3(0, -1, 2)   // Too far
        };
        var map = GenerateMap(points);
        var astar = new AStarEx(map);

        var results = astar.GetPath(TestMover, new Vector3(0, 0, 0), new Vector3(1, 1, 2));
        Assert.IsEqual(results.Length, 3);
        for (var i = 0; i < results.Length; i++)
        {
            Assert.IsEqual(results[i], points[i]);
        }
    }

    private IsoMap GenerateMap(List<Vector3> tilePositions)
    {
        return new IsoMap(tilePositions.Select(pos =>
        {
            var tile = new Entity();
            tile.AddComponent(new TileLocation() { TilePosition = pos });
            tile.AddComponent(new Terrain() { Type = TerrainType.Even });
            return tile;
        }).ToList());
    }

    private IsoMap GenerateMapWithTerrain(List<Vector3> tilePositions, List<TerrainType> tileTerrains)
    {
        var tiles = new List<Entity>();
        for (var i = 0; i < tilePositions.Count; i++)
        {
            var tile = new Entity();
            tile.AddComponent(new TileLocation() { TilePosition = tilePositions[i] });
            tile.AddComponent(new Terrain() { Type = tileTerrains[i] });
            tiles.Add(tile);
        }

        return new IsoMap(tiles);
    }
}