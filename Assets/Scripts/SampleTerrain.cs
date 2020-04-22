﻿using System;
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
    private List<Tilemap> tilemap_devCameraPath;

    // Reference to manager
    private SampleTerrainManager manager;

    // Objects for storing the tiles
    public Layer wall;
    public Layer wallDetail;
    public Layer background;
    public Layer hazard;
    public Layer ground;

    // Reference to the entry tile
    public Vector2Int entryTilePosition;
    private Vector2Int entryTilePositionLocal;
    public List<Exit> exitTilePositions;

    // Terrain direction and type
    public TerrainManager.TerrainDirection direction;
    public SampleTerrainType terrainType;

    public List<SampleItem> items;

    [Range(0, 1)] public float itemChance = 1;

    public void LoadSample()
    {
        // References 
        manager = transform.root.GetComponent<SampleTerrainManager>();
        grid = GetComponent<Grid>();

        exitTilePositions = new List<Exit>();
        items = new List<SampleItem>();

        tilemap_devCameraPath = new List<Tilemap>();

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
                // Add the camera path tilemaps
                if (g.name.Contains("Camera"))
                {
                    tilemap_devCameraPath.Add(t);
                }
                else
                {
                    tilemap_dev = t;
                }

            }

            // Disable the rendering of all samples
            r.enabled = false;
        }

        // Create new objects to store the tile data
        wall = new Layer();
        wallDetail = new Layer();
        background = new Layer();
        hazard = new Layer();
        ground = new Layer();

        // Find the entry tile (must be first)
        FindEntryTilePosition(ref entryTilePositionLocal);
        entryTilePosition = Vector2Int.zero;

        // Find all the exit tile positions
        FindExitTilePositions(ref exitTilePositions);
        // And their extra camera paths 
        FindCameraPathPositions(ref exitTilePositions, tilemap_devCameraPath);

        // Load the item types and positions
        LoadItems(ref items, tilemap_dev);

        // Load all the tiles in the tilemaps into the objects
        LoadTiles(tilemap_wall, ref wall);
        LoadTiles(tilemap_wallDetail, ref wallDetail);
        LoadTiles(tilemap_background, ref background);
        LoadTiles(tilemap_hazard, ref hazard);
        LoadTiles(tilemap_ground, ref ground);
    }



    /// <summary>
    /// Get the bounds in tiles for the playable area in this Sample Terrain.
    /// </summary>
    public GroundBounds GetGroundBounds()
    {
        // Get array just so we can initialise the variables to the first element 
        Layer.SampleTile[] tiles = ground.tilesInThisLayer.ToArray();

        Vector2Int min = tiles[0].position, max = tiles[0].position;

        // Might as well use the array since we have it lol
        foreach (Layer.SampleTile tile in tiles)
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



    private void LoadTiles(Tilemap tilemap, ref Layer layer)
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
                layer.tilesInThisLayer.Add(new Layer.SampleTile(t, new Vector2Int(current.x, current.y) - entryTilePositionLocal));
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
                if (tilemap_dev.GetTile(current).Equals(manager.dev_entryLeft))
                {
                    direction = TerrainManager.TerrainDirection.Left;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_entryRight))
                {
                    direction = TerrainManager.TerrainDirection.Right;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_entryUp))
                {
                    direction = TerrainManager.TerrainDirection.Up;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_entryDown))
                {
                    direction = TerrainManager.TerrainDirection.Down;
                }
                else
                {
                    // Do nothing
                    continue;
                }
                // Set the tile and return 
                tile = new Vector2Int(current.x, current.y);
                return;
            }
        }
        throw new Exception("Entry tile could not be found in SampleTerrain.");
    }

    /// <summary>
    /// Must be called AFTER entry position has been assigned
    /// </summary>
    private void FindExitTilePositions(ref List<Exit> exits)
    {
        exits.Clear();

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
                exits.Add(new Exit(direction, tile));
            }
        }

        if (exits.Count == 0)
        {
            throw new Exception("Could not find any exit tiles in SampleTerrain.");
        }
    }


    private void FindCameraPathPositions(ref List<Exit> exits, List<Tilemap> cameraPaths)
    {
        if (exits.Count == 0)
        {
            throw new Exception("Sample Terrain has no exits");
        }
        if (cameraPaths.Count == 0)
        {
            throw new Exception("Sample Terrain has no camera path tilemaps");
        }

        foreach (Tilemap tilemap in cameraPaths)
        {
            // Get an iterator for the bounds of the tilemap 
            BoundsInt.PositionEnumerator p = tilemap.cellBounds.allPositionsWithin.GetEnumerator();
            while (p.MoveNext())
            {
                // Ensure a valid tile
                Vector3Int current = p.Current;
                TileBase t = tilemap.GetTile(current);
                if (t != null)
                {
                    ExitDirection d = ExitDirection.Right;
                    // Check direction
                    if (t.Equals(manager.dev_cameraPathLeft))
                    {
                        d = ExitDirection.Left;
                    }
                    else if (t.Equals(manager.dev_cameraPathRight))
                    {
                        d = ExitDirection.Right;
                    }
                    else if (t.Equals(manager.dev_cameraPathUp))
                    {
                        d = ExitDirection.Up;
                    }
                    else if (t.Equals(manager.dev_cameraPathDown))
                    {
                        d = ExitDirection.Down;
                    }
                    else
                    {
                        continue;
                    }

                    // Find the exit
                    foreach (Exit e in exits)
                    {
                        if (e.exitDirection.Equals(d))
                        {
                            // Add the extra point to it
                            e.cameraPathPoints.Add(new Vector2Int(current.x, current.y) - entryTilePositionLocal);
                            break;
                        }

                    }
                }
            }
        }

        // Sort the lists by distance from entry tile pos
        foreach (Exit exit in exits)
        {
            exit.cameraPathPoints.Sort((x, y) => Vector2Int.Distance(entryTilePosition, x).CompareTo(Vector2Int.Distance(entryTilePosition, y)));
        }
    }



    private void LoadItems(ref List<SampleItem> items, Tilemap tilemap)
    {
        items.Clear();

        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            TileBase t = tilemap.GetTile(current);
            if (t != null)
            {
                InteractableItem.Name itemType = InteractableItem.Name.Coin;
                Vector2Int tilePos = new Vector2Int(current.x, current.y) - entryTilePositionLocal;

                // Assign the correct type
                if(t.Equals(manager.dev_itemCoin))
                {
                    itemType = InteractableItem.Name.Coin;
                }
                else if (t.Equals(manager.dev_itemPot))
                {
                    itemType = InteractableItem.Name.Pot;
                }
                else if (t.Equals(manager.dev_itemChest))
                {
                    itemType = InteractableItem.Name.Chest;
                }
                else
                {
                    // If not an item, do nothing
                    continue;
                }

                // Add the new item
                items.Add(new SampleItem(itemType, tilePos));
            }
        }
    }



    /// <summary>
    /// Class for storing the tiles in a specific layer of Sample Terrain.
    /// </summary>
    public class Layer
    {
        public List<SampleTile> tilesInThisLayer;

        public Layer()
        {
            tilesInThisLayer = new List<SampleTile>();
        }

        /// <summary>
        /// Class for storing an induvidual tile of Sample Terrain.
        /// </summary>
        public class SampleTile
        {
            public TileBase tileType;
            public Vector2Int position;

            public SampleTile(TileBase tileType, Vector2Int position)
            {
                this.tileType = tileType;
                this.position = position;
            }
        }
    }

    public class SampleItem
    {
        public InteractableItem.Name type;
        public Vector2Int tilePos;

        public SampleItem(InteractableItem.Name type, Vector2Int tilePos)
        {
            this.type = type;
            this.tilePos = tilePos;
        }
    }


    public class Exit
    {
        /// <summary>
        /// The direction of the exit, relative to the enterance
        /// </summary>
        public ExitDirection exitDirection;
        public Vector2Int exitPositionRelative;
        public List<Vector2Int> cameraPathPoints;

        public Exit(ExitDirection exitDirection, Vector2Int exitPositionRelative)
        {
            this.exitDirection = exitDirection;
            this.exitPositionRelative = exitPositionRelative;

            cameraPathPoints = new List<Vector2Int>();
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
