using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class TerrainManager : MonoBehaviour
{
    public static UnityAction<Vector2Int> OnTerrainGenerated;

    [Header("General Generation Settings")]
    public string seed;
    public bool useRandomSeed;

    [Header("General Tilemap settings")]
    public int widthInTiles = 128;
    public int heightInTiles = 128;

    [Header("Ground Generation Settings")]
    public int newTileMaxOffsetX = 6;
    public int newTileMaxOffsetY = 3;

    [Header("Tilemap References")]
    public Tilemap wall;
    public Tilemap wallDetail;
    public Tilemap background;
    public Tilemap ground;
    private List<Tilemap> allTilemaps;

    [Header("Tile Prefabs")]
    public Tile groundTile;
    public Tile wallTileMain;
    public Tile wallTileDetail;

    private Vector2Int lastGeneratedTile;

    private System.Random random;


    private void Awake()
    {
        // Set up the random generator
        int seedHash = seed.GetHashCode();
        // Get a random seed
        if (useRandomSeed)
        {

        }
        random = new System.Random(seedHash);

        // Set up tilemaps 
        SetupTilemaps();
    }


    public void Generate()
    {
        ClearAllTiles();

        Vector2Int initialTile = GenerateInitialTile();

        for (int i = 0; i < 20; i++)
        {
            GenerateNewTile();
        }

        Debug.Log("generate called. initial tile: " + initialTile.x + ", " + initialTile.y);
        OnTerrainGenerated.Invoke(initialTile);
    }

    private void SetupTilemaps()
    {
        // Add all the tilemaps to the list
        allTilemaps = new List<Tilemap>();
        allTilemaps.Add(wall);
        allTilemaps.Add(wallDetail);
        allTilemaps.Add(background);
        allTilemaps.Add(ground);

        // Set some general settings for each
        foreach (Tilemap t in allTilemaps)
        {
            /*
            t.size = new Vector3Int(widthInTiles, heightInTiles, 0);
            Vector3 pos = transform.position;
            pos.x -= (widthInTiles * t.cellSize.x);
            pos.y -= (heightInTiles * t.cellSize.y);

            t.transform.position = pos;
            */
        }
    }

    private void ClearAllTiles()
    {
        // Remove all tiles
        foreach (Tilemap t in allTilemaps)
        {
            t.ClearAllTiles();
        }
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

