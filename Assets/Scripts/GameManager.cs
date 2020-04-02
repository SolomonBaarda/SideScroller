using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static UnityAction OnGameStart;

    public string seed;
    public bool useRandomSeed;

    [Header("Tilemap settings")]
    public int widthInTiles = 128;
    public int heightInTiles = 128;

    //[Header("Wall Settings")]

    [Header("Ground Generation Settings")]
    public int newTileMaxOffsetX = 6;
    public int newTileMaxOffsetY = 3;

    private Vector2Int lastGeneratedTile;

    [Header("Tilemaps")]
    public Tilemap wall;
    public Tilemap wallDetail;
    public Tilemap background;
    public Tilemap ground;
    private List<Tilemap> allTilemaps;

    [Header("Tiles")]
    public Tile groundTile;
    public Tile wallTileMain;
    public Tile wallTileDetail;


    [Header("Player")]
    public GameObject playerGameObject;
    private Player player;


    private System.Random random; 
    private void Awake()
    {
        int seedHash = seed.GetHashCode();
        // Get a random seed
        if (useRandomSeed)
        {
            
        }
        random = new System.Random(seedHash);

        SetupTilemaps();

        player = playerGameObject.GetComponent<Player>();

        // Move the player to the initial tile
        Vector2Int initialTile = GenerateInitialTile();
        Vector3 initialWorldPosition = ground.CellToWorld(new Vector3Int(initialTile.x, initialTile.y, 0));

        // Get the position of the top, centre of the tile
        initialWorldPosition.x += ground.cellSize.x / 2;
        initialWorldPosition.y += ground.cellSize.y;
        // Move the player to that position
        player.SetPosition(new Vector2(initialWorldPosition.x, initialWorldPosition.y));

        for(int i = 0; i < 20; i++)
        {
            GenerateNewTile();
        }
    }


    public Vector2Int GenerateNewTile()
    {
        Vector2Int newTile = lastGeneratedTile;

        newTile.x += random.Next(1, newTileMaxOffsetX);
        newTile.y += random.Next(-newTileMaxOffsetY, newTileMaxOffsetY);

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


    private void SetupTilemaps()
    {
        allTilemaps = new List<Tilemap>();
        allTilemaps.Add(wall);
        allTilemaps.Add(wallDetail);
        allTilemaps.Add(background);
        allTilemaps.Add(ground);

        foreach (Tilemap t in allTilemaps)
        {
            t.size = new Vector3Int(widthInTiles, heightInTiles, 0);
            Vector3 pos = transform.position;
            pos.x -= (widthInTiles * t.cellSize.x);
            pos.y -= (heightInTiles * t.cellSize.y);

            t.transform.position = pos;
        }
    }


    private void ClearAllTiles()
    {
        foreach (Tilemap t in allTilemaps)
        {
            t.ClearAllTiles();
        }
    }


}
