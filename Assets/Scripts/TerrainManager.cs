using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;


public class TerrainManager : MonoBehaviour
{
    public static UnityAction OnTerrainGenerated;

    [Header("General Generation Settings")]
    public string seed;
    public bool useRandomSeed;

    [Header("Ground Generation Settings")]
    public int newTileMaxOffsetX = 6;
    public int newTileMaxOffsetY = 3;


    [Header("Grid Reference")]
    public Grid grid;
    [HideInInspector]
    public Tilemap wall;
    [HideInInspector]
    public Tilemap wallDetail;
    [HideInInspector]
    public Tilemap background;
    [HideInInspector]
    public Tilemap ground;

    [Header("Tile Prefabs")]
    public Tile groundTile;
    public Tile wallTileMain;
    public Tile wallTileDetail;

    [Header("Sample Terrain Manager Reference")]
    public GameObject sampleTerrainManagerObject;
    private SampleTerrainManager sampleTerrainManager;

    //[Header("Terrain Chunk Samples")]

    [HideInInspector]
    public Vector2Int initialTile;
    private Vector2Int lastGeneratedTile;

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

        }
        random = new System.Random(seedHash);

        sampleTerrainManager = sampleTerrainManagerObject.GetComponent<SampleTerrainManager>();

        // Assign the tilemaps semi-dynamically 
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


    public void Generate()
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

        foreach (SampleTerrain t in sampleTerrainManager.allSamples)
        {
            for (int i = 0; i < 10; i++)
            {
                lastGeneratedTile = CopySampleTerrain(lastGeneratedTile, t);
            }

        }


        after = DateTime.Now;
        time = after - before;
        Debug.Log("It took " + time.Milliseconds + " ms to generate the starting area.");

        OnTerrainGenerated.Invoke();
    }


    private Vector2Int CopySampleTerrain(Vector2Int entryPosition, SampleTerrain terrain)
    {
        CopySampleTerrainLayer(entryPosition, terrain.wall, ref wall);
        CopySampleTerrainLayer(entryPosition, terrain.wallDetail, ref wallDetail);
        CopySampleTerrainLayer(entryPosition, terrain.background, ref background);
        CopySampleTerrainLayer(entryPosition, terrain.ground, ref ground);

        return entryPosition + terrain.exitTilePosition;
    }

    private void CopySampleTerrainLayer(Vector2Int entryPosition, SampleTerrain.SampleTerrainLayer layer, ref Tilemap tilemap)
    {
        // Copy wall
        foreach (SampleTerrain.SampleTerrainLayer.SampleTerrainTile tile in layer.tilesInThisLayer)
        {
            Vector3Int newTilePos = new Vector3Int(entryPosition.x, entryPosition.y, wall.cellBounds.z) +
                new Vector3Int(tile.position.x, tile.position.y, wall.cellBounds.z);

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




    private Vector2Int GenerateNewTile()
    {
        Vector2Int newTile = lastGeneratedTile;

        // Get a random position
        newTile.x += random.Next(1, newTileMaxOffsetX);
        newTile.y += random.Next(-newTileMaxOffsetY, newTileMaxOffsetY);

        // Set the new tilw
        SetTile(ground, groundTile, newTile);

        return newTile;
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
        // Update the last set tile
        lastGeneratedTile = tilePosition;
    }



}


