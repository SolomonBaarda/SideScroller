using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrain : MonoBehaviour 
{
    // Reference to manager
    private SampleTerrainManager manager;

    // Tilemaps 
    private Grid grid;
    private Tilemap tilemap_wall;
    private Tilemap tilemap_wallDetail;
    private Tilemap tilemap_background;
    private Tilemap tilemap_hazard;
    private Tilemap tilemap_ground;
    private Tilemap tilemap_dev;
    private List<Tilemap> tilemap_dev_AllCameraPaths;

    private GameObject extraObjectsParent;
    /// <summary>
    /// List of extra GameObjects and their positions in tiles, relative to the entry tile.
    /// </summary>
    public List<(GameObject, Vector2)> extraGameObjects;

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
    public List<Spawn> extraRespawnPoints;
    public List<Vector2Int> endTileLocations = null;
    // Item stuff
    public List<Item> items;
    [Range(0, 1)] public float itemChance = 1;

    // Terrain direction and type
    public TerrainManager.Direction direction;

    public int index;


    private void Awake()
    {
        // References 
        manager = transform.root.GetComponent<SampleTerrainManager>();

        grid = GetComponentInChildren<Grid>();

        // Destroy all TilemapRenderers connected
        foreach(TilemapRenderer r in GetComponentsInChildren<TilemapRenderer>())
        {
            Destroy(r);
        }



        extraObjectsParent = transform.Find("Extras").gameObject;

        // Disable all extras for now
        for(int i = 0; i < extraObjectsParent.transform.childCount; i++)
        {
            GameObject g = extraObjectsParent.transform.GetChild(i).gameObject;

            if(g != null)
            {
                g.SetActive(false);
            }
        }
    }

    public void LoadSample(int index)
    {
        this.index = index;

        exitTilePositions = new List<Exit>();
        items = new List<Item>();
        extraRespawnPoints = new List<Spawn>();
        endTileLocations = new List<Vector2Int>();

        tilemap_dev_AllCameraPaths = new List<Tilemap>();

        // Assign the tilemaps 
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject g = grid.transform.GetChild(i).gameObject;
            Tilemap t = g.GetComponent<Tilemap>();
            TilemapRenderer r = g.GetComponent<TilemapRenderer>();

            if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_WALL))
            {
                if(r.sortingOrder == 0)
                {
                    tilemap_wall = t;
                }
                else if (r.sortingOrder > 0)
                {
                    tilemap_wallDetail = t;
                }

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
                    tilemap_dev_AllCameraPaths.Add(t);
                }
                else
                {
                    tilemap_dev = t;
                }
            }
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
        FindCameraPathPositions(ref exitTilePositions, tilemap_dev_AllCameraPaths);
        // Load any extra respawn points
        FindRespawnPoints(ref extraRespawnPoints);
        FindEndBounds(ref endTileLocations);

        // Load the item types and positions
        LoadItems(ref items, tilemap_dev);

        // Load all the tiles in the tilemaps into the objects
        LoadTiles(tilemap_wall, ref wall);
        LoadTiles(tilemap_wallDetail, ref wallDetail);
        LoadTiles(tilemap_background, ref background);
        LoadTiles(tilemap_hazard, ref hazard);
        LoadTiles(tilemap_ground, ref ground);

        // Load all other objects - Lights etc.
        extraGameObjects = new List<(GameObject, Vector2)>();
        for (int i = 0; i < extraObjectsParent.transform.childCount; i++)
        {
            GameObject g = extraObjectsParent.transform.GetChild(i).gameObject;

            if (g != null)
            {
                if(g.activeInHierarchy)
                {
                    // Add the object and its relative position to the entry tile
                    Vector2 objectPosition = g.transform.position - grid.CellToWorld(new Vector3Int(entryTilePositionLocal.x, entryTilePositionLocal.y, 0));
                    extraGameObjects.Add((g, objectPosition / grid.cellSize));
                }
            }
        }
    }



    /// <summary>
    /// Get the bounds in tiles for the playable area in this Sample Terrain.
    /// </summary>
    public GroundBounds GetGroundBounds()
    {
        // Get array just so we can initialise the variables to the first element 
        Layer.Tile[] tiles = ground.tilesInThisLayer.ToArray();

        Vector2Int min = tiles[0].position, max = tiles[0].position;

        // Might as well use the array since we have it lol
        foreach (Layer.Tile tile in tiles)
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
                layer.tilesInThisLayer.Add(new Layer.Tile(t, new Vector2Int(current.x, current.y) - entryTilePositionLocal));
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
                    direction = TerrainManager.Direction.Left;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_entryRight))
                {
                    direction = TerrainManager.Direction.Right;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_entryUp))
                {
                    direction = TerrainManager.Direction.Up;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_entryDown))
                {
                    direction = TerrainManager.Direction.Down;
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



    private void FindRespawnPoints(ref List<Spawn> respawns)
    {
        respawns.Clear();

        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap_dev.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap_dev.GetTile(current) != null)
            {
                Payload.Direction direction;

                // Check if it is an exit tile type
                if (tilemap_dev.GetTile(current).Equals(manager.dev_spawnBoth))
                {
                    direction = Payload.Direction.None;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_spawnRight))
                {
                    direction = Payload.Direction.Right;
                }
                else if (tilemap_dev.GetTile(current).Equals(manager.dev_spawnLeft))
                {
                    direction = Payload.Direction.Left;
                }
                // Do nothing if not (for now)
                else
                {
                    continue;
                }

                // Add the new exit
                Vector2Int tile = new Vector2Int(current.x, current.y) - entryTilePositionLocal;
                respawns.Add(new Spawn(direction, tile));
            }
        }
    }



    private void FindEndBounds(ref List<Vector2Int> points)
    {
        points.Clear();

        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap_dev.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap_dev.GetTile(current) != null)
            {
                // Check if it is a finish tile, and if so add it
                if (tilemap_dev.GetTile(current).Equals(manager.dev_finish))
                {
                    points.Add(new Vector2Int(current.x, current.y) - entryTilePositionLocal);
                }                
            }
        }
    }


    private void LoadItems(ref List<Item> items, Tilemap tilemap)
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
                WorldItem.Name itemType = WorldItem.Name.Coin;
                Vector2Int tilePos = new Vector2Int(current.x, current.y) - entryTilePositionLocal;

                // Assign the correct type
                if(t.Equals(manager.dev_itemCoin))
                {
                    itemType = WorldItem.Name.Coin;
                }
                else if (t.Equals(manager.dev_itemPot))
                {
                    itemType = WorldItem.Name.Pot;
                }
                else if (t.Equals(manager.dev_itemChest))
                {
                    itemType = WorldItem.Name.Chest;
                }
                else
                {
                    // If not an item, do nothing
                    continue;
                }

                // Add the new item
                items.Add(new Item(itemType, tilePos));
            }
        }
    }



    /// <summary>
    /// Class for storing the tiles in a specific layer of Sample Terrain.
    /// </summary>
    public class Layer
    {
        public List<Tile> tilesInThisLayer;

        public Layer()
        {
            tilesInThisLayer = new List<Tile>();
        }

        /// <summary>
        /// Class for storing an induvidual tile of Sample Terrain.
        /// </summary>
        public class Tile
        {
            public TileBase tileType;
            public Vector2Int position;

            public Tile(TileBase tileType, Vector2Int position)
            {
                this.tileType = tileType;
                this.position = position;
            }
        }
    }


    public class Item
    {
        public WorldItem.Name type;
        public Vector2Int tilePos;

        public Item(WorldItem.Name type, Vector2Int tilePos)
        {
            this.type = type;
            this.tilePos = tilePos;
        }
    }


    public class Spawn
    {
        public Payload.Direction direction;
        public Vector2Int tilePos;

        public Spawn(Payload.Direction direction, Vector2Int tilePos)
        {
            this.direction = direction;
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



    public enum ExitDirection
    {
        Left,
        Right,
        Up,
        Down
    }


}
