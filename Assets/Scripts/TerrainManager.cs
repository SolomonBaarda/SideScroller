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

        Vector3 tileRight = grid.CellToWorld(new Vector3Int(initialTile.x + 1, initialTile.y, 0));
        Vector3 tileLeft = grid.CellToWorld(new Vector3Int(initialTile.x - 1, initialTile.y, 0));

        // Generate the spawn room
        GenerateFromSampleTerrain(initialTile, TerrainDirection.Both, sampleTerrainManager.startingArea, Vector2Int.zero);

        after = DateTime.Now;
        time = after - before;
        Debug.Log("It took " + time.Milliseconds + " ms to generate the starting area.");

        OnTerrainGenerated.Invoke();
    }


    public void Generate(Vector3 startTileWorldSpace, TerrainDirection directionToGenerate, Vector2Int chunkID)
    {
        // Calculate what we are going to generate 
        int index = random.Next(0, sampleTerrainManager.allSamples.Length);
        SampleTerrain chosen = sampleTerrainManager.allSamples[index];

        Vector3Int entryPos = grid.WorldToCell(startTileWorldSpace);

        // Generate the new chunk and update the tile reference
        GenerateFromSampleTerrain(new Vector2Int(entryPos.x, entryPos.y), directionToGenerate, chosen, chunkID);
    }




    private void GenerateFromSampleTerrain(Vector2Int entryTile, TerrainDirection directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        bool flipAxisX = false;

        // Check if we need to invert the x pos of the tile
        if (!directionToGenerate.Equals(terrain.direction))
        {
            flipAxisX = true;
        }

        // Copy the terrain, each layer at a time
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wall, ref wall);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wallDetail, ref wallDetail);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.background, ref background);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.hazard, ref hazard);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.ground, ref ground);

        // Tell the ChunkManager that the terrain has been generated
        OnTerrainChunkGenerated.Invoke(GenerateTerrainChunk(entryTile, flipAxisX, directionToGenerate, terrain, chunkID));
    }

    private TerrainChunk GenerateTerrainChunk(Vector2Int entryTile, bool flipAxisX, TerrainDirection directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        // Get the inverse multiplier
        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        // Calculate some important values
        SampleTerrain.GroundBounds b = terrain.GetGroundBounds();
        Vector3 entryPositionWorld = grid.CellToWorld(new Vector3Int(entryTile.x, entryTile.y, 0)) + (grid.cellSize / 2);

        // Centre position
        Vector2Int centreTile = entryTile + new Vector2Int(b.minTile.x * invert, b.minTile.y) + new Vector2Int(b.boundsTile.x * invert / 2, b.boundsTile.y / 2);
        Vector3 centre = grid.CellToWorld(new Vector3Int(centreTile.x, centreTile.y, 0));
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
        List<Chunk.ChunkExit> exits = new List<Chunk.ChunkExit>();
        foreach (SampleTerrain.SampleTerrainExit sampleExit in terrain.exitTilePositions)
        {
            // Calculate where the exit should be
            Vector2Int exitTile = entryTile + new Vector2Int(invert * sampleExit.exitPositionRelative.x, sampleExit.exitPositionRelative.y);
            Vector3 exitPositionWorld = grid.CellToWorld(new Vector3Int(exitTile.x, exitTile.y, 0)) + (grid.cellSize / 2);

            // Move by 1 tile in correct direction
            Vector2Int newChunkTile = exitTile;
            Vector2Int newChunkID = chunkID;
            SampleTerrain.ExitDirection exitDirection = SampleTerrain.ExitDirection.Horizontal;
            TerrainDirection newChunkDirection = directionToGenerate;

            switch (sampleExit.exitDirection)
            {
                // Exit is up
                case SampleTerrain.ExitDirection.Up:
                    newChunkTile.y++;
                    newChunkID.y++;
                    exitDirection = SampleTerrain.ExitDirection.Up;
                    break;
                // Exit is down
                case SampleTerrain.ExitDirection.Down:
                    newChunkTile.y--;
                    newChunkID.y--;
                    exitDirection = SampleTerrain.ExitDirection.Down;
                    break;
                    // Horizontal, need to check which type
                case SampleTerrain.ExitDirection.Horizontal:
                    switch (directionToGenerate)
                    {
                        // Exit is left
                        case TerrainDirection.Left:
                            newChunkTile.x--;
                            newChunkID.x--;
                            break;
                        // Exit is right
                        case TerrainDirection.Right:
                            newChunkTile.x++;
                            newChunkID.x++;
                            break;
                        // Exits both ways, need to check both
                        case TerrainDirection.Both:
                            if (exitTile.x > entryTile.x)
                            {
                                newChunkTile.x++;
                                newChunkID.x++;
                                newChunkDirection = TerrainDirection.Right;
                            }
                            else if (exitTile.x < entryTile.x)
                            {
                                newChunkTile.x--;
                                newChunkID.x--;
                                newChunkDirection = TerrainDirection.Left;
                            }
                            break;
                    }
                    break;
            }

            // World position of the start of the new chunk
            Vector3 newChunkPositionWorld = grid.CellToWorld(new Vector3Int(newChunkTile.x, newChunkTile.y, 0)) + (grid.cellSize / 2);

            // Add the exit to list of exits for this chunk
            exits.Add(new Chunk.ChunkExit(exitDirection, exitPositionWorld, newChunkPositionWorld, newChunkDirection, newChunkID));
        }

        // Return the TerrainChunk object for use in the ChunkManager
        return new TerrainChunk(b.boundsReal, grid.cellSize, centre, entryPositionWorld, exits, directionToGenerate, chunkID);
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


    public Vector3 GetInitialTileWorldPositionForPlayer()
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
        Both
    }


    /// <summary>
    /// Class between Sample Terrain and Chunk.  Used to store values when passed as an event.
    /// </summary>
    public class TerrainChunk
    {
        public Vector2 bounds;
        public Vector3 cellSize;
        public Vector3 centre;
        public Vector3 enteranceWorldPosition;
        public List<Chunk.ChunkExit> exits;
        public TerrainDirection direction;
        public Vector2Int chunkID;

        public Vector3 cameraPathStartWorldSpace;
        public Vector3 cameraPathEndWorldSpace;


        public TerrainChunk(Vector2 bounds, Vector3 cellSize, Vector3 centre, Vector3 enteranceWorldPosition,
                List<Chunk.ChunkExit> exits, TerrainDirection direction, Vector2Int chunkID)
        {
            this.bounds = bounds;
            this.cellSize = cellSize;
            this.centre = centre;
            this.enteranceWorldPosition = enteranceWorldPosition;
            this.exits = exits;
            this.direction = direction;
            this.chunkID = chunkID;
        }
    }

}


