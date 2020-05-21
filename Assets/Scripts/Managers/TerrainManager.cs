using Pathfinding.Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;

public class TerrainManager : MonoBehaviour
{
    /// <summary>
    ///  
    /// </summary>
    public static UnityAction OnSpawnGenerated;

    /// <summary>
    /// Called when a terrain chunk has been generated.  
    /// </summary>
    public static UnityAction<TerrainChunk> OnTerrainChunkGenerated;

    [Header("General Generation Settings")]
    public string seed;
    public bool useRandomSeed;

    public const int DEAULT_WORLD_LENGTH_NOT_ENDLESS = 8;
    public int WorldLength { get; private set; } = DEAULT_WORLD_LENGTH_NOT_ENDLESS;

    public Generation GenerationRule { get; private set; }

    public Vector2 CellSize { get { return grid.cellSize; } }

    public static readonly Vector2Int initialTilePos = Vector2Int.zero;

    // Tilemaps
    private Grid grid;
    private Tilemap wall;
    private Tilemap wallDetail;
    private Tilemap background;
    private Tilemap hazard;
    private Tilemap ground;

    [Header("Sample Terrain Manager Reference")]
    public GameObject sampleTerrainManagerObjectPrefab;
    private SampleTerrainManager sampleTerrainManager;

    [Header("Initial Tile Type")]
    public Tile groundTile;
    public RandomTile backgroundWallTile;

    private System.Random random;

    // Layers
    public const string LAYER_NAME_WALL = "Wall";
    public const string LAYER_NAME_WALL_DETAIL = "Wall Detail";
    public const string LAYER_NAME_BACKGROUND = "Background";
    public const string LAYER_NAME_HAZARD = "Hazard";
    public const string LAYER_NAME_GROUND = "Ground";
    public const string LAYER_NAME_DEV = "Dev";

    private void Awake()
    {
        // Set up the random generator
        int seedHash = seed.GetHashCode();
        // Get a random seed
        if (useRandomSeed)
        {
            seedHash = Environment.TickCount;
        }
        random = new System.Random(seedHash);


        // Get the references
        grid = GetComponent<Grid>();

        GameObject sampleGameObject = Instantiate(sampleTerrainManagerObjectPrefab);
        sampleTerrainManager = sampleGameObject.GetComponent<SampleTerrainManager>();

        // Assign the tilemaps
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject g = grid.transform.GetChild(i).gameObject;
            Tilemap t = g.GetComponent<Tilemap>();
            TilemapRenderer r = g.GetComponent<TilemapRenderer>();

            if (r.sortingLayerName.Equals(LAYER_NAME_WALL))
            {
                wall = t;
            }
            else if (r.sortingLayerName.Equals(LAYER_NAME_WALL_DETAIL))
            {
                wallDetail = t;
            }
            else if (r.sortingLayerName.Equals(LAYER_NAME_BACKGROUND))
            {
                background = t;
            }
            else if (r.sortingLayerName.Equals(LAYER_NAME_HAZARD))
            {
                hazard = t;
            }
            else if (r.sortingLayerName.Equals(LAYER_NAME_GROUND))
            {
                ground = t;
            }
        }
    }



    public void GenerateSpawn(Generation worldgenerationType, int worldLength = DEAULT_WORLD_LENGTH_NOT_ENDLESS)
    {
        GenerationRule = worldgenerationType;
        WorldLength = worldLength;

        // Reset the tilemaps 
        ClearAllTiles();

        // Generate a single tile at the orign
        GenerateInitialTile(initialTilePos);

        // Generate the spawn area
        Generate(initialTilePos, Direction.Both, ChunkManager.initialChunkID, sampleTerrainManager.startingArea);

        OnSpawnGenerated.Invoke();
    }


    public void LoadSampleTerrain(bool printDebug)
    {
        // Load the sample terrain
        DateTime before = DateTime.Now;

        sampleTerrainManager.LoadAllSampleTerrain();

        DateTime after = DateTime.Now;
        TimeSpan time = after - before;
        if (printDebug)
        {
            Debug.Log("It took " + time.Milliseconds + " ms to load the sample terrain.");
        }
    }



    public void GenerateRandom(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID)
    {
        // Get a list of only the valid sample terrain
        List<SampleTerrain> allValidSamples = new List<SampleTerrain>();

        // Get all possible terrain
        foreach (SampleTerrain t in sampleTerrainManager.allSamples)
        {
            if (IsValidDirection(directionToGenerate, t.direction))
            {
                allValidSamples.Add(t);
            }
        }

        // Check if there is a limit
        if (GenerationRule.ToString().Contains("Limit"))
        {
            // Get the current world length in chunks
            int currentWorldLength = Mathf.Abs(chunkID.x - ChunkManager.initialChunkID.x) + Mathf.Abs(chunkID.y - ChunkManager.initialChunkID.y);

            // This is the final chunk to generate
            if (currentWorldLength == WorldLength)
            {
                // Ensure it is only finishing areas that get generated
                allValidSamples = new List<SampleTerrain>
                {
                    sampleTerrainManager.finishArea
                };
            }
            // We have generated enough chunks
            else if (currentWorldLength > WorldLength)
            {
                return;
            }
        }

        if (allValidSamples.Count.Equals(0))
        {
            throw new Exception("No valid Sample Terrain for generation direction " + directionToGenerate);
        }


        GenerateRandom(startTileWorldSpace, directionToGenerate, chunkID, allValidSamples);
    }


    public void GenerateRandom(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID, List<SampleTerrain> validTerrain)
    {
        // Randomly choose one to generate
        int index = random.Next(0, validTerrain.Count);
        SampleTerrain chosen = validTerrain[index];

        Generate(startTileWorldSpace, directionToGenerate, chunkID, chosen);
    }


    public void Generate(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID, SampleTerrain terrain)
    {
        // Decide if we need to flip the terrain on the x axis
        bool flipAxisX = false;
        if ((directionToGenerate.Equals(Direction.Left) && terrain.direction.Equals(Direction.Right))
            || (directionToGenerate.Equals(Direction.Right) && terrain.direction.Equals(Direction.Left)))
        {
            flipAxisX = true;
        }

        Vector3Int entryPos = grid.WorldToCell(startTileWorldSpace);

        // Generate the new chunk and update the tile reference
        GenerateFromSampleTerrain(new Vector2Int(entryPos.x, entryPos.y), flipAxisX, directionToGenerate, terrain, chunkID);
    }




    private void GenerateFromSampleTerrain(Vector2Int entryTile, bool flipAxisX, Direction directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        // Copy the terrain, each layer at a time
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wall, ref wall);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wallDetail, ref wallDetail);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.background, ref background);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.hazard, ref hazard);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.ground, ref ground);

        // Generate the chunk
        TerrainChunk c = GenerateTerrainChunk(entryTile, flipAxisX, directionToGenerate, terrain, chunkID);


        // Add some walls
        /*
        Vector3Int centre = grid.WorldToCell(c.centre);
        Vector2Int extents = new Vector2Int((int)(c.bounds / c.cellSize / 2).x, (int)(c.bounds / c.cellSize / 2).y);
        Vector3Int offset = new Vector3Int();


        int offsetDistance = 4;

        if (directionToGenerate == Direction.Left || directionToGenerate == Direction.Right || directionToGenerate == Direction.Both)
        {
            offset.y = (int)(c.bounds.y / 2) + offsetDistance;
        }

        //wall.BoxFill(centre, backgroundWallTile, centre.x -extents.x, centre.y + offset.y, centre.x + extents.x, centre.y + offset.y);
        //wall.BoxFill(centre, backgroundWallTile, centre.x - extents.x, centre.y - offset.y, centre.x + extents.x, centre.y - offset.y);

        // Do a flood fill of wall tiles
        wall.FloodFill(centre + offset, backgroundWallTile);
        wall.FloodFill(centre - offset, backgroundWallTile);
        */

        // Tell the ChunkManager that the terrain has been generated
        OnTerrainChunkGenerated.Invoke(c);
    }


    private TerrainChunk GenerateTerrainChunk(Vector2Int entryTile, bool flipAxisX, Direction directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        // Get the inverse multiplier
        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        // Calculate some important values
        SampleTerrain.GroundBounds b = terrain.GetGroundBounds();
        Vector2 entryPositionWorldCorner = grid.CellToWorld(new Vector3Int(entryTile.x, entryTile.y, 0));
        Vector2 entryPositionWorld = entryPositionWorldCorner + (CellSize / 2);

        // Centre position
        Vector2Int centreTile = entryTile + new Vector2Int(b.minTile.x * invert, b.minTile.y) + new Vector2Int(b.boundsTile.x * invert / 2, b.boundsTile.y / 2);
        Vector2 centre = grid.CellToWorld(new Vector3Int(centreTile.x, centreTile.y, 0));

        // Need to add half a cell for odd numbers
        if (b.boundsTile.x % 2 == 1) { centre.x += CellSize.x / 2; }
        if (b.boundsTile.y % 2 == 1) { centre.y += CellSize.y / 2; }

        // Add extra respawn points 
        List<TerrainChunk.Respawn> respawnPoints = new List<TerrainChunk.Respawn>();
        respawnPoints.Add(new TerrainChunk.Respawn(Payload.Direction.None, entryPositionWorld));

        // Add all the extra respawn points
        foreach (SampleTerrain.Spawn s in terrain.extraRespawnPoints)
        {
            Payload.Direction dir = s.direction;

            if (invert == -1)
            {
                if (dir == Payload.Direction.Left)
                {
                    dir = Payload.Direction.Right;
                }
                else if (dir == Payload.Direction.Right)
                {
                    dir = Payload.Direction.Left;
                }
            }

            Vector2Int respawnTile = entryTile + new Vector2Int(invert * s.tilePos.x, s.tilePos.y);
            Vector2 spawnPos = (Vector2)grid.CellToWorld(new Vector3Int(respawnTile.x, respawnTile.y, 0)) + CellSize / 2;
            respawnPoints.Add(new TerrainChunk.Respawn(dir, spawnPos));
        }

        // Loop through each sample terrain exit
        List<TerrainChunk.Exit> exits = new List<TerrainChunk.Exit>();
        foreach (SampleTerrain.Exit sampleExit in terrain.exitTilePositions)
        {
            // Calculate where the exit should be
            Vector2Int exitTile = entryTile + new Vector2Int(invert * sampleExit.exitPositionRelative.x, sampleExit.exitPositionRelative.y);
            Vector2 exitPositionWorld = grid.CellToWorld(new Vector3Int(exitTile.x, exitTile.y, 0)) + (grid.cellSize / 2);

            Vector2Int newChunkTile = exitTile;
            Vector2Int newChunkID = chunkID;
            Direction newChunkDirection = directionToGenerate;

            // Move by 1 tile in correct direction
            switch (sampleExit.exitDirection)
            {
                // Exit is left
                case SampleTerrain.ExitDirection.Left:
                    newChunkTile.x += flipAxisX ? 1 : -1;
                    newChunkID.x += flipAxisX ? 1 : -1;
                    newChunkDirection = flipAxisX ? Direction.Right : Direction.Left;
                    break;
                // Exit is right
                case SampleTerrain.ExitDirection.Right:
                    newChunkTile.x += !flipAxisX ? 1 : -1;
                    newChunkID.x += !flipAxisX ? 1 : -1;
                    newChunkDirection = !flipAxisX ? Direction.Right : Direction.Left;
                    break;
                // Exit is up
                case SampleTerrain.ExitDirection.Up:
                    newChunkTile.y++;
                    newChunkID.y++;
                    newChunkDirection = Direction.Up;
                    break;
                // Exit is down
                case SampleTerrain.ExitDirection.Down:
                    newChunkTile.y--;
                    newChunkID.y--;
                    newChunkDirection = Direction.Down;
                    break;
            }

            // World position of the start of the new chunk
            Vector2 newChunkPositionWorld = (Vector2)grid.CellToWorld(new Vector3Int(newChunkTile.x, newChunkTile.y, 0)) + (CellSize / 2);

            Vector2Int tilesAwayFromOrigin = exitTile - initialTilePos;

            // Make the exit
            TerrainChunk.Exit e = new TerrainChunk.Exit(newChunkDirection, exitPositionWorld, newChunkPositionWorld, newChunkID, tilesAwayFromOrigin);

            // Get the camera path points
            foreach (Vector2Int point in sampleExit.cameraPathPoints)
            {
                // Get the world pos
                Vector3Int pointTile = new Vector3Int(entryTile.x + invert * point.x, entryTile.y + point.y, 0);
                Vector2 worldPos = (Vector2)grid.CellToWorld(pointTile) + CellSize / 2;

                // Add the point 
                e.cameraPathPoints.Add(worldPos);
            }

            // Sort the lists by distance from entry tile pos
            e.cameraPathPoints.Sort((x, y) => Vector2.Distance(entryPositionWorld, x).CompareTo(Vector2.Distance(entryPositionWorld, y)));

            // Add the exit to list of exits for this chunk
            exits.Add(e);
        }

        List<TerrainChunk.Item> allItemPositions = new List<TerrainChunk.Item>();

        // Calculate the Items
        foreach (SampleTerrain.Item item in terrain.items)
        {
            // Get position of the centre of the tile
            Vector2 pos = (Vector2)grid.CellToWorld(new Vector3Int(entryTile.x + invert * item.tilePos.x, entryTile.y + item.tilePos.y, 0)) + (CellSize / 2);
            // And add it
            allItemPositions.Add(new TerrainChunk.Item(item.type, pos));
        }

        // Get all extra world objects
        List<(GameObject, Vector2, Vector3)> extraWorldObjects = new List<(GameObject, Vector2, Vector3)>();
        for(int i = 0; i < terrain.extraGameObjects.Count; i++)
        {
            // Calculate the new position and rotation
            Vector2 position = entryPositionWorldCorner + (new Vector2(invert * terrain.extraGameObjects[i].Item2.x, terrain.extraGameObjects[i].Item2.y) * CellSize);
            Vector3 localScale = terrain.extraGameObjects[i].Item1.transform.localScale;

            // The object needs to be flipped
            if (invert < 0)
            {
                position.x += CellSize.x;

                localScale.x *= -1;
            }
            
            extraWorldObjects.Add((terrain.extraGameObjects[i].Item1, position, localScale));
        }

        // Get the finishing bounds if this chunk is a finish
        TerrainChunk.Finish f = new TerrainChunk.Finish();
        if(terrain.endTileLocations != null)
        {
            if(terrain.endTileLocations.Count > 0)
            {
                Vector2Int defaultValue = entryTile + terrain.endTileLocations[0];
                Vector2Int minTile = defaultValue, maxTile = defaultValue;
                foreach (Vector2Int exitTile in terrain.endTileLocations)
                {
                    // Set the min and max points
                    Vector2Int tile = entryTile + exitTile;
                    minTile.x = tile.x < minTile.x ? tile.x : minTile.x;
                    minTile.y = tile.y < minTile.y ? tile.y : minTile.y;

                    maxTile.x = tile.x > maxTile.x ? tile.x : maxTile.x;
                    maxTile.y = tile.y > maxTile.y ? tile.y : maxTile.y;
                }

                Vector2 min = grid.CellToWorld(new Vector3Int(minTile.x, minTile.y, 0));
                Vector2 max = (Vector2)grid.CellToWorld(new Vector3Int(maxTile.x, maxTile.y, 0)) + CellSize;
                Vector2 size = max - min;

                f = new TerrainChunk.Finish(new Bounds(min + (size / 2), size));
            }
        }


        // Return the TerrainChunk object for use in the ChunkManager
        return new TerrainChunk(b.boundsReal, CellSize, centre, entryPositionWorld, exits, respawnPoints, directionToGenerate, chunkID,
            allItemPositions, terrain.itemChance, extraWorldObjects, f, terrain.index);
    }



    private void CopySampleTerrainLayer(Vector2Int entryPosition, bool flipAxisX, SampleTerrain.Layer layer, ref Tilemap tilemap)
    {
        Vector2Int entry = new Vector2Int(entryPosition.x, entryPosition.y);

        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        // Copy wall
        foreach (SampleTerrain.Layer.Tile tile in layer.tilesInThisLayer)
        {
            // Position of the new tile
            Vector2Int newTilePos = entry + new Vector2Int(invert * tile.position.x, tile.position.y);

            TileBase newTileType = tile.tileType;

            // Check if we need to flip the tile type
            if(invert < 0)
            {
                if(tile.tileType is RuleTile)
                {
                    // Swap the direction
                    if(tile.tileType.Equals(sampleTerrainManager.rampLeft))
                    {
                        newTileType = sampleTerrainManager.rampRight;
                    }
                    else if (tile.tileType.Equals(sampleTerrainManager.rampRight))
                    {
                        newTileType = sampleTerrainManager.rampLeft;
                    }
                }
            }

            SetTile(tilemap, newTileType, newTilePos);
        }
    }


    private bool IsValidDirection(Direction directionToGenerate, Direction sampleTerrainDirection)
    {
        // Terrain is already the correct direction
        return directionToGenerate.Equals(sampleTerrainDirection)
            // Terrain can go both ways
            || (directionToGenerate.Equals(Direction.Both) && (sampleTerrainDirection.Equals(Direction.Left) || (sampleTerrainDirection.Equals(Direction.Right))))
            // Terrain will need to be flipped
            || (directionToGenerate.Equals(Direction.Left) && sampleTerrainDirection.Equals(Direction.Right))
            || (directionToGenerate.Equals(Direction.Right) && sampleTerrainDirection.Equals(Direction.Left));
    }

    public void ClearAllTiles()
    {
        wall.ClearAllTiles();
        wallDetail.ClearAllTiles();
        background.ClearAllTiles();
        ground.ClearAllTiles();
    }


    public Vector2 GetInitialTileWorldPositionForPlayer()
    {
        // Get the initial position + (half a cell, 1 cell, 0) to point to the top, middle of the cell
        return ground.CellToWorld(new Vector3Int(initialTilePos.x, initialTilePos.y, ground.cellBounds.z)) + new Vector3(ground.cellSize.x / 2, ground.cellSize.y, 0);
    }


    private Vector2Int GenerateInitialTile(Vector2Int tilePos)
    {
        // Calculate the initial position
        SetTile(ground, groundTile, tilePos);

        return tilePos;
    }


    private void SetTile(Tilemap tilemap, TileBase tileType, Vector2Int tilePosition)
    {
        // Set the cell
        tilemap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), tileType);
    }


    public Vector2Int GetGroundBoundsTiles()
    {
        Vector3Int bounds = ground.cellBounds.size;
        return new Vector2Int(bounds.x, bounds.y);
    }

    public Vector2Int GetTilePosition(Vector2 position)
    {
        Vector3Int tile = grid.WorldToCell(position);
        return new Vector2Int(tile.x, tile.y);
    }


    public SampleTerrain GetSampleTerrain(int sampleTerrainID)
    {
        return sampleTerrainManager.allSamples[sampleTerrainID];
    }


    public enum Generation
    {
        Multidirectional_Endless,
        Symmetrical_Endless,
        Symmetrical_Limit,
    }


    /// <summary>
    /// The direction that the terrain is facing. Camera should move in that direction.
    /// </summary>
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        Both
    }


    /// <summary>
    /// Class between Sample Terrain and Chunk.  Used to store values when passed as an event.
    /// </summary>
    public struct TerrainChunk
    {
        public Vector2 bounds;
        public Vector2 cellSize;
        public Vector2 centre;

        public Vector2 enteranceWorldPosition;
        public List<Exit> exits;
        public List<Respawn> respawnPoints;

        public Direction direction;
        public Vector2Int chunkID;

        public List<Item> items;
        public float itemChance;

        public List<(GameObject, Vector2, Vector3)> extraWorldObjects;

        public int sampleIndex;

        public Finish finishArea;

        public TerrainChunk(Vector2 bounds, Vector2 cellSize, Vector2 centre, Vector2 enteranceWorldPosition, List<Exit> exits, List<Respawn> respawnPoints,
            Direction direction, Vector2Int chunkID, List<Item> items, float itemChance, List<(GameObject, Vector2, Vector3)> extraWorldObjects, Finish finishArea, 
            int sampleIndex)
        {
            this.bounds = bounds;
            this.cellSize = cellSize;
            this.centre = centre;
            this.enteranceWorldPosition = enteranceWorldPosition;
            this.exits = exits;
            this.respawnPoints = respawnPoints;
            this.direction = direction;
            this.chunkID = chunkID;
            this.items = items;
            this.itemChance = itemChance;
            this.extraWorldObjects = extraWorldObjects;
            this.finishArea = finishArea;
            this.sampleIndex = sampleIndex;
        }

        public struct Exit
        {
            public Direction exitDirection;
            public Vector2 exitPositionWorld;

            public Vector2 newChunkPositionWorld;
            public Vector2Int newChunkID;

            public List<Vector2> cameraPathPoints;

            public Vector2Int tilesFromOrigin;

            public Exit(Direction exitDirection, Vector2 exitPositionWorld, Vector2 newChunkPositionWorld, Vector2Int newChunkID, Vector2Int tilesFromOrigin)
            {
                this.exitDirection = exitDirection;
                this.exitPositionWorld = exitPositionWorld;
                this.newChunkPositionWorld = newChunkPositionWorld;
                this.newChunkID = newChunkID;

                cameraPathPoints = new List<Vector2>();

                this.tilesFromOrigin = tilesFromOrigin;
            }
        }

        public struct Respawn
        {
            public Payload.Direction direction;
            public Vector2 position;

            public Respawn(Payload.Direction direction, Vector2 position)
            {
                this.direction = direction;
                this.position = position;
            }
        }


        public struct Item
        {
            public WorldItem.Name itemType;
            public Vector2 centreOfTile;

            public Item(WorldItem.Name itemType, Vector2 centreOfTile)
            {
                this.itemType = itemType;
                this.centreOfTile = centreOfTile;
            }
        }


        public struct Finish
        {
            public bool isFinish;
            public Bounds bounds;


            public Finish(Bounds bounds)
            {
                this.bounds = bounds;
                isFinish = true;
            }
        }
    }



}


