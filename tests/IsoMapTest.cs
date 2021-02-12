using Ecs;
using Godot;
using System.Collections.Generic;

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
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(1, 2, 3) });
        var map = new IsoMap(new List<Entity>() { tile });

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

        Assert.IsEqual(map.MapToWorld(new Vector3(1, 0, 0)), new Vector2(IsoMap.TileWidth, IsoMap.TileHeight));

        Assert.IsEqual(map.MapToWorld(new Vector3(0, 1, 0)), new Vector2(-IsoMap.TileWidth, IsoMap.TileHeight));

        Assert.IsEqual(map.MapToWorld(new Vector3(0, 0, 1)), new Vector2(0, -IsoMap.TileThickness));
    }

    [Test]
    public void Pick_ReturnsEmptyList_NoTileUnderWorldPosition()
    {
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(10, 10, 10) });
        var map = new IsoMap(new List<Entity>() { tile });

        var results = map.Pick(new Vector2(99999, 99999));
        Assert.IsEqual(results.Count, 0);
    }

    [Test]
    public void Pick_ReturnsSingleTile_SingleTileUnderWorldPosition()
    {
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(10, 10, 10) });
        var tile2 = new Entity();
        tile2.AddComponent(new TileLocation() { TilePosition = new Vector3(1, 2, 3) });
        var map = new IsoMap(new List<Entity>() { tile, tile2 });

        var results = map.Pick(map.MapToWorld(new Vector3(10, 10, 10)));
        Assert.IsEqual(results.Count, 1);
    }

    [Test]
    public void Pick_ReturnsCorrectTile_ExtentsOfTileBounds()
    {
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(0, 0, 0) });
        var map = new IsoMap(new List<Entity>() { tile });

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
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(10, 10, 10) });
        var tile2 = new Entity();
        tile2.AddComponent(new TileLocation() { TilePosition = new Vector3(9, 9, 8) });
        var map = new IsoMap(new List<Entity>() { tile, tile2 });

        var results = map.Pick(map.MapToWorld(new Vector3(10, 10, 10)));
        Assert.IsEqual(results.Count, 2);
    }

    [Test]
    public void PickUncovered_ReturnsSameAsPick_NoCoveredTiles()
    {
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(10, 10, 10) });
        var tile2 = new Entity();
        tile2.AddComponent(new TileLocation() { TilePosition = new Vector3(9, 9, 8) });
        var map = new IsoMap(new List<Entity>() { tile, tile2 });

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
    public void PickUncovered_AdjustsTilesPicked_CoveredTile()
    {
        var tile = new Entity();
        tile.AddComponent(new TileLocation() { TilePosition = new Vector3(9, 9, 9) });
        var tile2 = new Entity();
        tile2.AddComponent(new TileLocation() { TilePosition = new Vector3(9, 9, 8) });
        var map = new IsoMap(new List<Entity>() { tile, tile2 });

        var pickVector = map.MapToWorld(new Vector3(10, 10, 10));
        var results = map.Pick(pickVector);
        var resultsUncovered = map.PickUncovered(pickVector);
        Assert.IsEqual(results.Count, 1);
        Assert.IsEqual(results[0], tile2);
        Assert.IsEqual(resultsUncovered.Count, 1);
        Assert.IsEqual(resultsUncovered[0], tile);
    }
}