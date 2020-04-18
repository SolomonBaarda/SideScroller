using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrain : MonoBehaviour
{
    // Tilemaps 
    private Grid grid;
    private Tilemap tilemap_wall;
    private Tilemap tilemap_wallDetail;
    private Tilemap tilemap_background;
    private Tilemap tilemap_hazard;
    private Tilemap tilemap_ground;
    private Tilemap tilemap_dev;

    // Reference to manager
    private SampleTerrainManager manager;

    // Objects for storing the tiles
    public SampleTerrainLayer wall;
    public SampleTerrainLayer wallDetail;
    public SampleTerrainLayer background;
    public SampleTerrainLayer hazard;
    public SampleTerrainLayer ground;

    // Reference to the entry tile
    public Vector2Int entryTilePosition;
    private Vector2Int entryTilePositionLocal;
    public List<SampleTerrainExit> exitTilePositions;

    public TerrainManager.TerrainDirection direction;

    public SampleTerrainType terrainType;

    private void Awake()
    {
        manager = transform.root.GetComponent<SampleTerrainManager>();

        grid = GetComponent<Grid>();

        exitTilePositions = new List<SampleTerrainExit>();

        // Assign the tilemaps 
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject g = grid.transform.GetChild(i).gameObject;
            Tilemap t = g.GetComponent<Tilemap>();
            TilemapRenderer r = g.GetComponent<TilemapRenderer>();

            if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_WALL))
            {
                tilemap_wall = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_WALL_DETAIL))
            {
                tilemap_wallDetail = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_BACKGROUND))
            {
                tilemap_background = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_HAZARD))
            {
                tilemap_hazard = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_GROUND))
            {
                tilemap_ground = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_DEV))
            {
                tilemap_dev = t;
            }

            // Disable the rendering of all samples
            r.enabled = false;
        }

        // Create new objects to store the tile data
        wall = new SampleTerrainLayer();
        wallDetail = new SampleTerrainLayer();
        background = new SampleTerrainLayer();
        hazard = new SampleTerrainLayer();
        ground = new SampleTerrainLayer();


        FindEntryTilePosition(ref entryTilePositionLocal);

        entryTilePosition = Vector2Int.zero;
        FindExitTilePositions(ref exitTilePositions);

        // Load all the tiles in the tilemaps into the objects
        LoadTiles(tilemap_wall, ref wall);
        LoadTiles(tilemap_wallDetail, ref wallDetail);
        LoadTiles(tilemap_background, ref background);
        LoadTiles(tilemap_hazard, ref hazard);
        LoadTiles(tilemap_ground, ref ground);

        if (direction.Equals(null))
        {
            throw new Exception("Sample Terrain direction has not been defined.");
        }
    }


    public Vector3 GetTileSize()
    {
        return grid.cellSize;
    }


    /// <summary>
    /// Get the bounds in tiles for the playable area in this Sample Terrain.
    /// </summary>
    public GroundBounds GetGroundBounds()
    {
        // Get array just so we can initialise the variables to the first element 
        SampleTerrainLayer.SampleTerrainTile[] tiles = ground.tilesInThisLayer.ToArray();

        Vector2Int min = tiles[0].position, max = tiles[0].position;

        // Might as well use the array since we have it lol
        foreach (SampleTerrainLayer.SampleTerrainTile tile in tiles)
        {
            if (tile.position.x < min.x)
            {
                min.x = tile.position.x;
            }
            if (tile.position.y < min.y)
            {
                min.y = tile.position.y;
            }

            if (tile.position.x > max.x)
            {
                max.x = tile.position.x;
            }
            if (tile.position.y > max.y)
            {
                max.y = tile.position.y;
            }
        }

        return new GroundBounds(min, max, grid.cellSize);
    }



    private void LoadTiles(Tilemap tilemap, ref SampleTerrainLayer layer)
    {
        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            // Get the tile
            TileBase t = tilemap.GetTile(current);
            if (t != null)
            {
                // Add it to the list of tiles for that layer
                layer.tilesInThisLayer.Add(new SampleTerrainLayer.SampleTerrainTile(t, new Vector2Int(current.x, current.y) - entryTilePositionLocal));
            }
        }
    }


    private void FindEntryTilePosition(ref Vector2Int tile)
    {
        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap_dev.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap_dev.GetTile(current) != null)
            {
                // Check if the tile matches the entry tile type
                if (tilemap_dev.GetTile(current).Equals(manager.dev_entryTile))
                {
                    tile = new Vector2Int(current.x, current.y);
                    return;
                }
            }
        }
        throw new Exception("Entry tile could not be found in SampleTerrain.");
    }

    /// <summary>
    /// Must be called AFTER entry position has been assigned
    /// </summary>
    private void FindExitTilePositions(ref List<SampleTerrainExit> tiles)
    {
        tiles.Clear();

        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap_dev.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap_dev.GetTile(current) != null)
            {
                ExitDirection direction;

                // Check if it is an exit tile type
                if (tilemap_dev.GetTile(current).Equals(manager.dev_exitLeft))
                {
                    direction = ExitDirection.Left;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_exitRight))
                {
                    direction = ExitDirection.Right;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_exitUp))
                {
                    direction = ExitDirection.Up;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_exitDown))
                {
                    direction = ExitDirection.Down;
                }
                // Do nothing if not (for now)
                else
                {
                    continue;
                }

                // Add the new exit
                Vector2Int tile = new Vector2Int(current.x, current.y) - entryTilePositionLocal;
                tiles.Add(new SampleTerrainExit(direction, tile));
            }
        }

        if (tiles.Count == 0)
        {
            throw new Exception("Could not find any exit tiles in SampleTerrain.");
        }
    }


    /// <summary>
    /// Class for storing the tiles in a specific layer of Sample Terrain.
    /// </summary>
    public class SampleTerrainLayer
    {
        public List<SampleTerrainTile> tilesInThisLayer;

        public SampleTerrainLayer()
        {
            tilesInThisLayer = new List<SampleTerrainTile>();
        }

        /// <summary>
        /// Class for storing an induvidual tile of Sample Terrain.
        /// </summary>
        public class SampleTerrainTile
        {
            public TileBase tileType;
            public Vector2Int position;

            public SampleTerrainTile(TileBase tileType, Vector2Int position)
            {
                this.tileType = tileType;
                this.position = position;
            }
        }

    }

    public class SampleTerrainExit
    {
        /// <summary>
        /// The direction of the exit, relative to the enterance
        /// </summary>
        public ExitDirection exitDirection;
        public Vector2Int exitPositionRelative;

        public SampleTerrainExit(ExitDirection exitDirection, Vector2Int exitPositionRelative)
        {
            this.exitDirection = exitDirection;
            this.exitPositionRelative = exitPositionRelative;
        }
    }



    /// <summary>
    /// The bounds of the ground tiles in a Sample Terrain. Values relative to the enterance position.
    /// </summary>
    public class GroundBounds
    {
        public Vector2Int minTile, maxTile;

        public Vector2Int boundsTile;
        public Vector2 boundsReal;

        public GroundBounds(Vector2Int minTile, Vector2Int maxTile, Vector2 cellSize)
        {
            this.minTile = minTile;
            this.maxTile = maxTile;

            boundsTile = maxTile - minTile + Vector2Int.one;
            boundsReal = boundsTile * cellSize;
        }


    }


    public enum SampleTerrainType
    {
        Terrain,
        Spawn
    }

    public enum ExitDirection
    {
        Left,
        Right,
        Up,
        Down
    }


}
