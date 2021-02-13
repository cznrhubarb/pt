using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class IsoMap
{
    public const float TileWidth = 96;
    public const float TileHeight = 48;
    public const float TileThickness = TileHeight / 2;

    public List<Entity> Tiles { get; set; }

    private int mapDepth = 0;
    private Dictionary<TileKey, int> tileGridLookup = new Dictionary<TileKey, int>();

    public IsoMap(List<Entity> tiles)
    {
        // assumptions to be made: 
        //  z > 0, maybe x >0 and y >0 as well
        //  all entities passed in have a TileLocation and have position set already
        
        Tiles = tiles;

        for (var i = 0; i < tiles.Count; i++)
        {
            var key = TileKey.From(tiles[i]);
            mapDepth = Mathf.Max(mapDepth, key.z + 1);

            tileGridLookup.Add(key, i);
        }
    }

    public Entity GetTileAt(int x, int y, int z)
    {
        var key = new TileKey(x, y, z);
        if (tileGridLookup.ContainsKey(key))
        {
            return Tiles[tileGridLookup[key]];
        }

        return null;
    }

    public Entity GetTileAt(Vector3 tilePosition) =>
        GetTileAt((int)tilePosition.x, (int)tilePosition.y, (int)tilePosition.z);

    public Vector2 MapToWorld(Vector3 tilePosition)
    {
        var mx = (tilePosition.x - tilePosition.y) * TileWidth / 2;
        var my = (tilePosition.x + tilePosition.y) * TileHeight / 2 - tilePosition.z * TileThickness;

        return new Vector2(mx, my);
    }

    // Returns the list of all tiles this world position pierces, with highest first
    public List<Entity> Pick(Vector2 worldPosition)
    {
        var results = new List<Entity>();

        var flatTx = worldPosition.x / TileWidth + worldPosition.y / TileHeight;
        var flatTy = -worldPosition.x / TileWidth + worldPosition.y / TileHeight;
        for (var tz = mapDepth - 1; tz >= 0; tz--)
        {
            var tx = (int)(flatTx + tz * TileThickness / TileHeight);
            var ty = (int)(flatTy + tz * TileThickness / TileHeight);

            // If (tx, ty, tz) exists, add it to the list of possible tiles
            var key = new TileKey(tx, ty, tz);
            if (tileGridLookup.ContainsKey(key))
            {
                results.Add(Tiles[tileGridLookup[key]]);
            }
        }

        GD.Print("Done");
        return results;
    }

    // Same as Pick, but if a tile has another directly over it, go up until we find an uncovered tile and replace the original with this
    public List<Entity> PickUncovered(Vector2 worldPosition)
    {
        var possibleTiles = Pick(worldPosition);
        for (var i = 0; i < possibleTiles.Count - 1; i++)
        {
            var firstKey = TileKey.From(possibleTiles[i]);
            for (var j = i + 1; j < possibleTiles.Count; j++)
            {
                var secondKey = TileKey.From(possibleTiles[j]);
                if (firstKey.x == secondKey.x && firstKey.y == secondKey.y)
                {
                    // We assume Pick is returning these sorted highest to lowest
                    possibleTiles.RemoveAt(j);
                    j--;
                }
            }
        }

        var uncoveredTiles = possibleTiles.Select(tile =>
        {
            var key = TileKey.From(tile);
            while (key.z < mapDepth - 1 && tileGridLookup.ContainsKey(new TileKey(key.x, key.y, key.z + 1)))
            {
                key.z += 1;
            }
            return Tiles[tileGridLookup[key]];
        }).ToList();

        return uncoveredTiles;
    }

    private class TileKey
    {
        public int x, y, z;

        public TileKey(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TileKey;
            return x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 3049;
                hash = hash * 5039 + x;
                hash = hash * 883 + y;
                hash = hash * 9719 + z;
                return hash;
            }
        }

        public static TileKey From(Entity ent)
        {
            var pos = ent.GetComponent<TileLocation>().TilePosition;
            return new TileKey((int)pos.x, (int)pos.y, (int)pos.z);
        }
    }
}
