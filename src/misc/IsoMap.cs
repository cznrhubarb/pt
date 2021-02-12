using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

//Tile Entity
//	TileLocation
//	SpriteWrap
//	Terrain

//Map Utility
//	Dictionary<int, Dictionary<int, Dictonary<int, Tile>>>
//	iterate
//	MapToWorld, WorldToMap

class IsoMap
{
    public const int TileWidth = 96;
    public const int TileHeight = 48;
    public const int TileThickness = TileHeight / 2;

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
            mapDepth = Mathf.Max(mapDepth, key.z);

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
        var worldPos = new Vector2((tilePosition.x - tilePosition.y) * TileWidth, (tilePosition.x + tilePosition.y) * TileHeight);
        worldPos.y -= tilePosition.z * TileThickness;

        return worldPos;
    }

    // Returns the list of all tiles this world position pierces, with highest first
    public List<Entity> Pick(Vector2 worldPosition)
    {
        var results = new List<Entity>();

        for (var tz = mapDepth - 1; tz >= 0; tz--)
        {
            var tx = (int)(worldPosition.y / TileWidth + tz * TileThickness / TileWidth);
            var ty = (int)(worldPosition.y / TileWidth - worldPosition.x / TileHeight + tz * TileThickness / TileWidth);

            // If (tx, ty, tz) exists, add it to the list of possible tiles
            var key = new TileKey(tx, ty, tz);
            if (tileGridLookup.ContainsKey(key))
            {
                results.Add(Tiles[tileGridLookup[key]]);
            }
        }

        return results;
    }

    // Same as Pick, but if a tile has another directly over it, go up until we find an uncovered tile and replace the original with this
    public List<Entity> PickUncovered(Vector2 worldPosition)
    {
        return Pick(worldPosition).Select(tile =>
        {
            var key = TileKey.From(tile);
            while (key.z < mapDepth - 1 && tileGridLookup.ContainsKey(new TileKey(key.x, key.y, key.z+1)))
            {
                key.z += 1;
            }
            return Tiles[tileGridLookup[key]];
        }).ToList();
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





//      tx = wy/TW + tzTT/TW
//      ty = wy/TW - wx/TW + tzTT/TW


// assuming TW = 8, some possible values of wp (4, 16) are
//          2, 1, 0                 2, 1, 0
//          2.25, 1.25, 1           2, 1, 1
//          2.5, 1.5, 2             2, 1, 2
//          2.75, 1.75, 3           2, 1, 3
//          3, 2, 4                 3, 2, 4
//          3.25, 2.25, 5           3, 2, 5
//          3.5, 2.5, 6             3, 2, 6


// Where z = 0
// wx = (tx - ty) * TW
//      tx = wx/TW + ty
// wy = (tx + ty) * TW/2
//      wy = (wx/TW + ty + ty) * TW/2
//      2wy/TW = wx/TW + 2ty
//      ty = wy/TW - wx/2TW

// Where z = 1
// wx = (tx - ty) * TW
//      tx = wx/TW + ty
// wy = (tx + ty) * TW/2 - TW/4
//      wy = (wx/TW + ty + ty) * TW/2 - TT
//      wy + TT = (wx/TW + ty + ty) * TW/2
//      2wy/TW + 2TT/TW = wx/TW + 2ty
//      2ty = 2wy/TW + 2TT/TW - wx/TW
//      ty = wy/TW + TT/TW - wx/TW

//      tx = wy/TW + TT/TW
//      ty = wy/TW + TT/TW - wx/TW


//      (2wy + TW/2) / TW = wx/TW + 2ty
//      2wy/TW + 1/2 = wx/TW + 2ty
//      2wy/TW + 1/2 - wx/TW = 2ty
//      ty = wy/TW + 1/4 - wx/2TW

// Where z = 2
// wx = (tx - ty) * TW
//      tx = wx/TW + ty
// wy = (tx + ty) * TW/2 - 2TT


// Where z = 3
// wx = (tx - ty) * TW
//      tx = wx/TW + ty
// wy = (tx + ty) * TW/2 - 3TT