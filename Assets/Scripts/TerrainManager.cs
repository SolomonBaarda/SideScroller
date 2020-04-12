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
    public static UnityAction<Grid, SampleTerrain, Vector2Int, Vector2Int> OnTerrainChunkGenerated;

    [Header("General Generation Settings")]
    public string seed;
    public bool useRandomSeed;

    private Grid grid;
    private Tilemap wall;
    private Tilemap wallDetail;
    private Tilemap background;
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

        // Generate one to the right
        Generate(tileRight, TerrainDirection.Right, Vector2Int.zero);

        // Left TODO

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
        GenerateNewTerrainChunk(new Vector2Int(entryPos.x, entryPos.y), directionToGenerate, chosen, chunkID);
    }




    private void GenerateNewTerrainChunk(Vector2Int entryPosition, TerrainDirection directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        // TODO check direction and invert blocks if going left
        if(directionToGenerate.Equals(null))
        {

        }

        // Copy the terrain, each layer at a time
        CopySampleTerrainLayer(entryPosition, terrain.wall, ref wall);
        CopySampleTerrainLayer(entryPosition, terrain.wallDetail, ref wallDetail);
        CopySampleTerrainLayer(entryPosition, terrain.background, ref background);
        CopySampleTerrainLayer(entryPosition, terrain.ground, ref ground);

        // Tell the ChunkManager that the terrain has been generated
        OnTerrainChunkGenerated.Invoke(grid, terrain, entryPosition, chunkID);
    }



    private void CopySampleTerrainLayer(Vector2Int entryPosition, SampleTerrain.SampleTerrainLayer layer, ref Tilemap tilemap)
    {
        // Copy wall
        foreach (SampleTerrain.SampleTerrainLayer.SampleTerrainTile tile in layer.tilesInThisLayer)
        {
            Vector3Int newTilePos = new Vector3Int(entryPosition.x, entryPosition.y, tilemap.cellBounds.z) +
                new Vector3Int(tile.position.x, tile.position.y, tilemap.cellBounds.z);

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


    public enum TerrainDirection
    {
        Left,
        Right
    }

}


