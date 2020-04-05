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
        DateTime before = DateTime.Now;

        ClearAllTiles();

        initialTile = GenerateInitialTile();

        for (int i = 0; i < 20; i++)
        {
            GenerateNewTile();
        }

        DateTime after = DateTime.Now;
        TimeSpan time = after - before;
        Debug.Log("It took " + time.Milliseconds + " ms to generate the starting area.");

        OnTerrainGenerated.Invoke();
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


