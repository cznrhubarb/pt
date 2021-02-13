using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class IsoMapTest : WAT.Test
{
    public override string Title()
    {
        return "IsoMap";
    }

    [Test]
    public void GetTileAtXYZ_ReturnsNull_NoTiles()
    {
        var map = new IsoMap(new List<Entity>());

        Assert.IsNull(map.GetTileAt(0, 0, 0));
    }

    [Test]
    public void GetTileAtXYZ_ReturnsTile_TileAtLocation()
    {
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(1, 2, 3) });
        var map = new IsoMap(new List<Entity>() { tile });

        Assert.IsEqual(map.GetTileAt(1, 2, 3), tile);
    }

    [Test]
    public void GetTileAtXYZ_ReturnsNull_NoTileAtLocation()
    {
        var map = new IsoMap(GenerateTilesAt(new List<Vector3>() { new Vector3(1, 2, 3) }));

        Assert.IsNull(map.GetTileAt(9, 9, 9));
    }

    [Test]
    public void MapToWorld_ReturnsCorrectPoint()
    {
        var map = new IsoMap(new List<Entity>());

        // +X              +Y
        //    \ |      |  /
        //   ___|      |___

        Assert.IsEqual(map.MapToWorld(new Vector3(0, 0, 0)), new Vector2(0, 0));

        Assert.IsEqual(map.MapToWorld(new Vector3(1, 0, 0)), new Vector2(IsoMap.TileWidth/2, IsoMap.TileHeight/2));

        Assert.IsEqual(map.MapToWorld(new Vector3(0, 1, 0)), new Vector2(-IsoMap.TileWidth/2, IsoMap.TileHeight/2));

        Assert.IsEqual(map.MapToWorld(new Vector3(0, 0, 1)), new Vector2(0, -IsoMap.TileThickness));
    }

    [Test]
    public void Pick_ReturnsEmptyList_NoTileUnderWorldPosition()
    {
        var map = new IsoMap(GenerateTilesAt(new List<Vector3>() { new Vector3(0, 0, 0) }));

        var results = map.Pick(new Vector2(IsoMap.TileWidth + 1, 0));
        Assert.IsEqual(results.Count, 0);
        results = map.Pick(new Vector2(-IsoMap.TileWidth - 1, 0));
        Assert.IsEqual(results.Count, 0);
        results = map.Pick(new Vector2(0, IsoMap.TileHeight + 1));
        Assert.IsEqual(results.Count, 0);
        results = map.Pick(new Vector2(0, -IsoMap.TileHeight - 1));
        Assert.IsEqual(results.Count, 0);
        results = map.Pick(new Vector2(IsoMap.TileWidth / 2 + 1, IsoMap.TileHeight / 2 + 1));
        Assert.IsEqual(results.Count, 0);
    }

    [Test]
    public void Pick_ReturnsCorrectTile_SingleTile()
    {
        var map = new IsoMap(GenerateTilesAt(new List<Vector3>() { new Vector3(3, 0, 2) }));

        var results = map.Pick(map.MapToWorld(new Vector3(3, 0, 2)));
        Assert.IsEqual(results.Count, 1);
    }

    [Test]
    public void Pick_ReturnsCorrectTile_ExtentsOfTileBounds()
    {
        var map = new IsoMap(GenerateTilesAt(new List<Vector3>() { new Vector3(0, 0, 0) }));

        var results = map.Pick(new Vector2(0, 0));
        Assert.IsEqual(results.Count, 1);

        results = map.Pick(new Vector2(-IsoMap.TileWidth / 2 + 1, 0));
        Assert.IsEqual(results.Count, 1);

        results = map.Pick(new Vector2(IsoMap.TileWidth / 2 - 1, 0));
        Assert.IsEqual(results.Count, 1);

        results = map.Pick(new Vector2(0, IsoMap.TileHeight / 2 - 1));
        Assert.IsEqual(results.Count, 1);

        results = map.Pick(new Vector2(0, -IsoMap.TileHeight / 2 + 1));
        Assert.IsEqual(results.Count, 1);
    }

    [Test]
    public void Pick_ReturnsMultipleTiles_MultipleTilesUnderWorldPosition()
    {
        var map = new IsoMap(GenerateTilesAt(new List<Vector3>() { new Vector3(10, 10, 10), new Vector3(9, 9, 8) }));

        var results = map.Pick(map.MapToWorld(new Vector3(10, 10, 10)));
        Assert.IsEqual(results.Count, 2);
    }

    [Test]
    public void PickUncovered_ReturnsSameAsPick_NoCoveredTiles()
    {
        var map = new IsoMap(GenerateTilesAt(new List<Vector3>() { new Vector3(10, 10, 10), new Vector3(9, 9, 8) }));

        var pickVector = map.MapToWorld(new Vector3(10, 10, 10));
        var results = map.Pick(pickVector);
        var resultsUncovered = map.PickUncovered(pickVector);
        Assert.IsEqual(results.Count, resultsUncovered.Count);
        for (int i = 0; i < results.Count; i++)
        {
            Assert.IsEqual(results[i], resultsUncovered[i]);
        }
    }

    [Test]
    public void PickUncovered_StripsCoveredTiles_OverlappingCoveredTile()
    {
        var tiles = GenerateTilesAt(new List<Vector3>() { new Vector3(9, 9, 9), new Vector3(9, 9, 8) });
        var map = new IsoMap(tiles);

        var pickVector = map.MapToWorld(new Vector3(9, 9, 8));
        var results = map.Pick(pickVector);
        var resultsUncovered = map.PickUncovered(pickVector);
        Assert.IsEqual(results.Count, 2);
        Assert.IsEqual(resultsUncovered.Count, 1);
        Assert.IsEqual(resultsUncovered[0], tiles[0]);
    }

    [Test]
    public void PickUncovered_ReturnsTop_DeepStack()
    {
        var tiles = GenerateTilesAt(new List<Vector3>()
        {
            new Vector3(9, 9, 9),
            new Vector3(9, 9, 8),
            new Vector3(9, 9, 7),
            new Vector3(9, 9, 6),
            new Vector3(9, 9, 5)
        });
        var map = new IsoMap(tiles);

        var resultsUncovered = map.PickUncovered(map.MapToWorld(new Vector3(9, 9, 5)));
        Assert.IsEqual(resultsUncovered.Count, 1);
        Assert.IsEqual(resultsUncovered[0], tiles[0]);
    }

    private List<Entity> GenerateTilesAt(List<Vector3> positions)
    {
        return positions.Select(pos =>
        {
            var tile = new Entity();
            tile.AddComponent(new TileLocation() { TilePosition = pos });
            return tile;
        }).ToList();
    }
}