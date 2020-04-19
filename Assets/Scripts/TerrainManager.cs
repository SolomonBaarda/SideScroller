using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;


public class TerrainManager : MonoBehaviour
{
    /// <summary>
    /// Called when the initial terrain has been generated at the start of the game.
    /// </summary>
    public static UnityAction OnTerrainGenerated;
    /// <summary>
    /// Called when a terrain chunk has been generated.  
    /// </summary>
    public static UnityAction<TerrainChunk> OnTerrainChunkGenerated;

    [Header("General Generation Settings")]
    public string seed;
    public bool useRandomSeed;

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

    private Vector2Int initialTile;

    private System.Random random;


    public static string LAYER_NAME_WALL = "Wall";
    public static string LAYER_NAME_WALL_DETAIL = "Wall Detail";
    public static string LAYER_NAME_BACKGROUND = "Background";
    public static string LAYER_NAME_HAZARD = "Hazard";
    public static string LAYER_NAME_GROUND = "Ground";
    public static string LAYER_NAME_DEV = "Dev";

    private void Awake()
    {
        // Set up the random generator
        int seedHash = seed.GetHashCode();
        // Get a random seed
        if (useRandomSeed)
        {
            // TODO
        }
        random = new System.Random(seedHash);

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



    public void Initialise()
    {
        // Load the sample terrain
        DateTime before = DateTime.Now;

        sampleTerrainManager.LoadAllSampleTerrain();

        DateTime after = DateTime.Now;
        TimeSpan time = after - before;
        Debug.Log("It took " + time.Milliseconds + " ms to load the sample terrain.");


        // Generate the terrain
        before = DateTime.Now;

        ClearAllTiles();
        initialTile = GenerateInitialTile();

        // Generate the spawn room
        GenerateFromSampleTerrain(initialTile, false, TerrainDirection.Both, sampleTerrainManager.startingArea, 0, Vector2Int.zero);

        after = DateTime.Now;
        time = after - before;
        Debug.Log("It took " + time.Milliseconds + " ms to generate the starting area.");

        OnTerrainGenerated.Invoke();
    }


    private bool IsValidDirection(TerrainDirection directionToGenerate, TerrainDirection sampleTerrainDirection)
    {
        // Terrain is already the correct direction
        return directionToGenerate.Equals(sampleTerrainDirection)
            // Terrain can go both ways
            || (directionToGenerate.Equals(TerrainDirection.Both) && (sampleTerrainDirection.Equals(TerrainDirection.Left) || (sampleTerrainDirection.Equals(TerrainDirection.Right))))
            // Terrain will need to be flipped
            || (directionToGenerate.Equals(TerrainDirection.Left) && sampleTerrainDirection.Equals(TerrainDirection.Right))
            || (directionToGenerate.Equals(TerrainDirection.Right) && sampleTerrainDirection.Equals(TerrainDirection.Left));
    }


    public void Generate(Vector2 startTileWorldSpace, TerrainDirection directionToGenerate, float distanceFromOrigin, Vector2Int chunkID)
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

        //Debug.Log("all samples " + sampleTerrainManager.allSamples.Length);
        //Debug.Log("valid samples " + allValidSamples.Count);

        if(allValidSamples.Count.Equals(0))
        {
            throw new Exception("No valid Sample Terrain for generation direction " + directionToGenerate);
        }

        // Randomly choose one to generate
        SampleTerrain[] validSamples = allValidSamples.ToArray();
        int index = random.Next(0, validSamples.Length);
        SampleTerrain chosen = validSamples[index];
        bool flipAxisX = false;
        if((directionToGenerate.Equals(TerrainDirection.Left) && chosen.direction.Equals(TerrainDirection.Right))
            || (directionToGenerate.Equals(TerrainDirection.Right) && chosen.direction.Equals(TerrainDirection.Left))) {
            flipAxisX = true;
        }

        Vector3Int entryPos = grid.WorldToCell(startTileWorldSpace);

        // Generate the new chunk and update the tile reference
        GenerateFromSampleTerrain(new Vector2Int(entryPos.x, entryPos.y), flipAxisX, directionToGenerate, chosen, distanceFromOrigin, chunkID);
    }




    private void GenerateFromSampleTerrain(Vector2Int entryTile, bool flipAxisX, TerrainDirection directionToGenerate, SampleTerrain terrain, float distanceFromOrigin, Vector2Int chunkID)
    {
        // Copy the terrain, each layer at a time
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wall, ref wall);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wallDetail, ref wallDetail);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.background, ref background);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.hazard, ref hazard);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.ground, ref ground);

        // Tell the ChunkManager that the terrain has been generated
        OnTerrainChunkGenerated.Invoke(GenerateTerrainChunk(entryTile, flipAxisX, directionToGenerate, terrain, distanceFromOrigin, chunkID));
    }

    private TerrainChunk GenerateTerrainChunk(Vector2Int entryTile, bool flipAxisX, TerrainDirection directionToGenerate, SampleTerrain terrain, float distanceFromOrigin, Vector2Int chunkID)
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
            centre.x += grid.cellSize.x / 2;
        }
        if (b.boundsTile.y % 2 == 1)
        {
            centre.y += grid.cellSize.y / 2;
        }


        // Loop through each sample terrain exit
        List<TerrainChunk.Exit> exits = new List<TerrainChunk.Exit>();
        foreach (SampleTerrain.SampleTerrainExit sampleExit in terrain.exitTilePositions)
        {
            // Calculate where the exit should be
            Vector2Int exitTile = entryTile + new Vector2Int(invert * sampleExit.exitPositionRelative.x, sampleExit.exitPositionRelative.y);
            Vector2 exitPositionWorld = grid.CellToWorld(new Vector3Int(exitTile.x, exitTile.y, 0)) + (grid.cellSize / 2);

            Vector2Int newChunkTile = exitTile;
            Vector2Int newChunkID = chunkID;
            TerrainDirection newChunkDirection = directionToGenerate;

            // Move by 1 tile in correct direction
            switch (sampleExit.exitDirection)
            {
                // Exit is left
                case SampleTerrain.ExitDirection.Left:
                    newChunkTile.x += flipAxisX ? 1 : -1;
                    newChunkID.x += flipAxisX ? 1 : -1;
                    newChunkDirection = flipAxisX ? TerrainDirection.Right : TerrainDirection.Left;
                    break;
                // Exit is right
                case SampleTerrain.ExitDirection.Right:
                    newChunkTile.x += !flipAxisX ? 1 : -1;
                    newChunkID.x += !flipAxisX ? 1 : -1;
                    newChunkDirection = !flipAxisX ? TerrainDirection.Right : TerrainDirection.Left;
                    break;
                // Exit is up
                case SampleTerrain.ExitDirection.Up:
                    newChunkTile.y++;
                    newChunkID.y++;
                    newChunkDirection = TerrainDirection.Up;
                    break;
                // Exit is down
                case SampleTerrain.ExitDirection.Down:
                    newChunkTile.y--;
                    newChunkID.y--;
                    newChunkDirection = TerrainDirection.Down;
                    break;
            }

            // World position of the start of the new chunk
            Vector2 newChunkPositionWorld = grid.CellToWorld(new Vector3Int(newChunkTile.x, newChunkTile.y, 0)) + (grid.cellSize / 2);

            // Add the exit to list of exits for this chunk
            exits.Add(new TerrainChunk.Exit(newChunkDirection, exitPositionWorld, newChunkPositionWorld, newChunkID));
        }

        // Return the TerrainChunk object for use in the ChunkManager
        return new TerrainChunk(b.boundsReal, grid.cellSize, centre, entryPositionWorld, exits, directionToGenerate, distanceFromOrigin, chunkID);
    }



    private void CopySampleTerrainLayer(Vector2Int entryPosition, bool flipAxisX, SampleTerrain.SampleTerrainLayer layer, ref Tilemap tilemap)
    {
        Vector3Int entry = new Vector3Int(entryPosition.x, entryPosition.y, 0);

        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        // Copy wall
        foreach (SampleTerrain.SampleTerrainLayer.SampleTerrainTile tile in layer.tilesInThisLayer)
        {
            // Position of the new tile
            Vector3Int newTilePos = entry + new Vector3Int(invert * tile.position.x, tile.position.y, 0);

            tilemap.SetTile(newTilePos, tile.tileType);
        }
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
        return ground.CellToWorld(new Vector3Int(initialTile.x, initialTile.y, ground.cellBounds.z)) + new Vector3(ground.cellSize.x / 2, ground.cellSize.y, 0);
    }


    private Vector2Int GenerateInitialTile()
    {
        // Calculate the initial position
        Vector2Int initialPosition = Vector2Int.zero;
        SetTile(ground, groundTile, initialPosition);

        return initialPosition;
    }


    private void SetTile(Tilemap tilemap, Tile tileType, Vector2Int tilePosition)
    {
        // Set the cell
        tilemap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), tileType);
    }


    /// <summary>
    /// The direction that the terrain is facing. Camera should move in that direction.
    /// </summary>
    public enum TerrainDirection
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
    public class TerrainChunk
    {
        public Vector2 bounds;
        public Vector2 cellSize;
        public Vector2 centre;

        public Vector2 enteranceWorldPosition;
        public List<Exit> exits;
        public TerrainDirection direction;
        public float distanceFromOrigin;
        public Vector2Int chunkID;

        public Vector2 cameraPathStartWorldSpace;
        public Vector2 cameraPathEndWorldSpace;


        public TerrainChunk(Vector2 bounds, Vector2 cellSize, Vector2 centre, Vector2 enteranceWorldPosition,
                List<Exit> exits, TerrainDirection direction, float distanceFromOrigin, Vector2Int chunkID)
        {
            this.bounds = bounds;
            this.cellSize = cellSize;
            this.centre = centre;
            this.enteranceWorldPosition = enteranceWorldPosition;
            this.exits = exits;
            this.direction = direction;
            this.distanceFromOrigin = distanceFromOrigin;
            this.chunkID = chunkID;
        }

        public class Exit
        {
            public TerrainDirection exitDirection;
            public Vector2 exitPositionWorld;

            public Vector2 newChunkPositionWorld;
            public Vector2Int newChunkID;

            public Exit(TerrainDirection exitDirection, Vector2 exitPositionWorld, Vector2 newChunkPositionWorld, Vector2Int newChunkID)
            {
                this.exitDirection = exitDirection;
                this.exitPositionWorld = exitPositionWorld;
                this.newChunkPositionWorld = newChunkPositionWorld;
                this.newChunkID = newChunkID;
            }
        }
    }



}


