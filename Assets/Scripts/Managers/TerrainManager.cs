﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TerrainManager : MonoBehaviour
{
    /// <summary>
    /// Called when the initial terrain has been generated at the start of the game.
    /// </summary>
    public static UnityAction OnInitialTerrainGenerated;
    /// <summary>
    /// Called when a terrain chunk has been generated.  
    /// </summary>
    public static UnityAction<TerrainChunk> OnTerrainChunkGenerated;

    [Header("General Generation Settings")]
    public string seed;
    public bool useRandomSeed;


    public RuleTile rule;


    public const int DEFAULT_MAX_CHUNKS_NOT_ENDLESS = 8;

    public Generation GenerationRule { get; private set; }

    public Vector2 CellSize { get { return grid.cellSize; } }

    private Grid grid;
    private Tilemap wall;
    private Tilemap wallDetail;
    private Tilemap background;
    private Tilemap hazard;
    private Tilemap ground;

    [Header("Sample Terrain Manager Reference")]
    public GameObject sampleTerrainManagerObject;
    private SampleTerrainManager sampleTerrainManager;

    [Header("Initial Tile Type")]
    public Tile groundTile;

    public static readonly Vector2Int initialTilePos = Vector2Int.zero;

    [SerializeField]
    private System.Random random;


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

        OnInitialTerrainGenerated += EMPTY_METHOD;

        // Get the references
        grid = GetComponent<Grid>();
        sampleTerrainManager = sampleTerrainManagerObject.GetComponent<SampleTerrainManager>();

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



    public void Initialise(Generation rule, bool printDebug)
    {
        // Load the sample terrain
        DateTime before = DateTime.Now;

        sampleTerrainManager.LoadAllSampleTerrain();

        DateTime after = DateTime.Now;
        TimeSpan time = after - before;
        if(printDebug)
        {
            Debug.Log("It took " + time.Milliseconds + " ms to load the sample terrain.");
        }


        // Generate the terrain
        before = DateTime.Now;

        GenerationRule = rule;
        ClearAllTiles();
        GenerateInitialTile(initialTilePos);

        // Generate the spawn room
        GenerateFromSampleTerrain(initialTilePos, false, Direction.Both, sampleTerrainManager.startingArea, ChunkManager.initialChunkID);

        after = DateTime.Now;
        time = after - before;
        if(printDebug)
        {
            Debug.Log("It took " + time.Milliseconds + " ms to generate the starting area.");
        }

        OnInitialTerrainGenerated.Invoke();
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



    public void Generate(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID, int sampleIndex = -1)
    {
        // Get a list of only the valid sample terrain
        List<SampleTerrain> allValidSamples = new List<SampleTerrain>();
        foreach (SampleTerrain t in sampleTerrainManager.allSamples)
        {
            if (IsValidDirection(directionToGenerate, t.direction))
            {
                allValidSamples.Add(t);
            }
        }

        if (allValidSamples.Count.Equals(0))
        {
            throw new Exception("No valid Sample Terrain for generation direction " + directionToGenerate);
        }

        // Randomly choose one to generate
        SampleTerrain[] validSamples = allValidSamples.ToArray();
        int index = random.Next(0, validSamples.Length);
        SampleTerrain chosen = validSamples[index];

        // Overwrite it if we need to
        if ((GenerationRule.Equals(Generation.Symmetrical_Endless) || GenerationRule.Equals(Generation.Symmetrical_Limit)) && sampleIndex != -1)
        {
            chosen = validSamples[sampleIndex];
        }

        bool flipAxisX = false;
        if ((directionToGenerate.Equals(Direction.Left) && chosen.direction.Equals(Direction.Right))
            || (directionToGenerate.Equals(Direction.Right) && chosen.direction.Equals(Direction.Left)))
        {
            flipAxisX = true;
        }

        Vector3Int entryPos = grid.WorldToCell(startTileWorldSpace);

        // Generate the new chunk and update the tile reference
        GenerateFromSampleTerrain(new Vector2Int(entryPos.x, entryPos.y), flipAxisX, directionToGenerate, chosen, chunkID);
    }




    private void GenerateFromSampleTerrain(Vector2Int entryTile, bool flipAxisX, Direction directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        // Copy the terrain, each layer at a time
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wall, ref wall);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wallDetail, ref wallDetail);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.background, ref background);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.hazard, ref hazard);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.ground, ref ground);

        // Tell the ChunkManager that the terrain has been generated
        OnTerrainChunkGenerated.Invoke(GenerateTerrainChunk(entryTile, flipAxisX, directionToGenerate, terrain, chunkID));
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
        Vector2 entryPositionWorld = grid.CellToWorld(new Vector3Int(entryTile.x, entryTile.y, 0)) + (grid.cellSize / 2);

        // Centre position
        Vector2Int centreTile = entryTile + new Vector2Int(b.minTile.x * invert, b.minTile.y) + new Vector2Int(b.boundsTile.x * invert / 2, b.boundsTile.y / 2);
        Vector2 centre = grid.CellToWorld(new Vector3Int(centreTile.x, centreTile.y, 0));
        // Need to add half a cell for odd numbers
        if (b.boundsTile.x % 2 == 1)
        {
            centre.x += CellSize.x / 2;
        }
        if (b.boundsTile.y % 2 == 1)
        {
            centre.y += CellSize.y / 2;
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
            Vector2 newChunkPositionWorld = grid.CellToWorld(new Vector3Int(newChunkTile.x, newChunkTile.y, 0)) + (grid.cellSize / 2);

            Vector2Int tilesAwayFromOrigin = exitTile - initialTilePos;

            // Make the exit
            TerrainChunk.Exit e = new TerrainChunk.Exit(newChunkDirection, exitPositionWorld, newChunkPositionWorld, newChunkID, tilesAwayFromOrigin);



            // Get the camera path points
            foreach (Vector2Int point in sampleExit.cameraPathPoints)
            {
                // Get the world pos
                Vector3Int pointTile = new Vector3Int(entryTile.x + invert * point.x, entryTile.y + point.y, 0);
                Vector2 worldPos = grid.CellToWorld(pointTile);

                // Add a little to centre it
                worldPos.x += CellSize.x / 2;
                worldPos.y += CellSize.y / 2;

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
        foreach (SampleTerrain.SampleItem item in terrain.items)
        {
            // Get position of the centre of the tile
            Vector2 pos = grid.CellToWorld(new Vector3Int(entryTile.x + invert * item.tilePos.x, entryTile.y + item.tilePos.y, 0)) + grid.cellSize / 2;
            // And add it
            allItemPositions.Add(new TerrainChunk.Item(item.type, pos));
        }

        // Return the TerrainChunk object for use in the ChunkManager
        return new TerrainChunk(b.boundsReal, CellSize, centre, entryPositionWorld, exits, directionToGenerate, chunkID, 
            allItemPositions, terrain.itemChance, terrain.index);
    }



    private void CopySampleTerrainLayer(Vector2Int entryPosition, bool flipAxisX, SampleTerrain.Layer layer, ref Tilemap tilemap)
    {
        Vector3Int entry = new Vector3Int(entryPosition.x, entryPosition.y, 0);

        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        // Copy wall
        foreach (SampleTerrain.Layer.SampleTile tile in layer.tilesInThisLayer)
        {
            // Position of the new tile
            Vector3Int newTilePos = entry + new Vector3Int(invert * tile.position.x, tile.position.y, 0);

            tilemap.SetTile(newTilePos, tile.tileType);
        }

        // Loop through each tile
        /*
        BoundsInt.PositionEnumerator p = tilemap.cellBounds.allPositionsWithin;
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            // Get the tile
            TileBase t = tilemap.GetTile(current);
            if (t != null)
            {
                if (t is RuleTile r)
                {
                    //r.UpdateNeighborPositions();
                }
            }
        }
        */
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


    private void SetTile(Tilemap tilemap, Tile tileType, Vector2Int tilePosition)
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

    /// <summary>
    /// Empty method. Used to ensure event calls are never null.
    /// </summary>
    private void EMPTY_METHOD()
    {
        
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
        public Direction direction;
        public Vector2Int chunkID;

        public List<Item> items;
        public float itemChance;

        public int sampleIndex;

        public TerrainChunk(Vector2 bounds, Vector2 cellSize, Vector2 centre, Vector2 enteranceWorldPosition, List<Exit> exits, 
            Direction direction, Vector2Int chunkID, List<Item> items, float itemChance, int sampleIndex)
        {
            this.bounds = bounds;
            this.cellSize = cellSize;
            this.centre = centre;
            this.enteranceWorldPosition = enteranceWorldPosition;
            this.exits = exits;
            this.direction = direction;
            this.chunkID = chunkID;
            this.items = items;
            this.itemChance = itemChance;
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
    }



}


