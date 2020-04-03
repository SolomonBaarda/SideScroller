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

    [Header("General Tilemap settings")]
    public int widthInTiles = 128;
    public int heightInTiles = 128;

    [Header("Ground Generation Settings")]
    public int newTileMaxOffsetX = 6;
    public int newTileMaxOffsetY = 3;

    [Header("Tilemap References")]
    public Terrain terrain = new Terrain();

    [Header("Tile Prefabs")]
    public Tile groundTile;
    public Tile wallTileMain;
    public Tile wallTileDetail;

    [Header("Terrain Chunk Samples")]
    public List<TerrainChunk> terrainChunks;

    [HideInInspector]
    public Vector2Int initialTile;
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
    }


    public void Generate()
    {
        DateTime before = DateTime.Now;

        terrain.ClearAllTiles();

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




    private Vector2Int GenerateNewTile()
    {
        Vector2Int newTile = lastGeneratedTile;

        // Get a random position
        newTile.x += random.Next(1, newTileMaxOffsetX);
        newTile.y += random.Next(-newTileMaxOffsetY, newTileMaxOffsetY);

        // Set the new tilw
        SetTile(terrain.ground, groundTile, newTile);

        return newTile;
    }



    private Vector2Int GenerateInitialTile()
    {
        // Calculate the initial position
        Vector2Int initialPosition = Vector2Int.zero;
        SetTile(terrain.ground, groundTile, initialPosition);

        return initialPosition;
    }


    private void SetTile(Tilemap tilemap, Tile tileType, Vector2Int tilePosition)
    {
        // Set the cell
        tilemap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), tileType);
        // Update the last set tile
        lastGeneratedTile = tilePosition;
    }


    [Serializable]
    public class Terrain
    {
        public Tilemap wall;
        public Tilemap wallDetail;
        public Tilemap background;
        public Tilemap ground;

        public void ClearAllTiles()
        {
            wall.ClearAllTiles();
            wallDetail.ClearAllTiles();
            background.ClearAllTiles();
            ground.ClearAllTiles();
        }
    }


    [Serializable]
    public class TerrainChunk
    {
        public Terrain terrain = new Terrain();
        public Vector2Int entryPosition;
        public Vector2Int exitPosition;
    }
}


