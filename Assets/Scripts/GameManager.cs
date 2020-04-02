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

    //[Header("Ground Settings")]

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

    private void Awake()
    {
        SetupTilemaps();

        player = playerGameObject.GetComponent<Player>();

        // Move the player to the initial tile
        Vector3Int initialTile = GenerateInitialTile();
        Vector3 initialWorldPosition = ground.CellToWorld(initialTile);

        // Get the position of the top, centre of the tile
        initialWorldPosition.x += ground.cellSize.x / 2;
        initialWorldPosition.y += ground.cellSize.y;
        // Move the player to that position
        player.SetPosition(new Vector2(initialWorldPosition.x, initialWorldPosition.y));
    }



    private void FixedUpdate()
    {

    }


    private Vector3Int GenerateInitialTile()
    {
        Vector3Int initialPosition = Vector3Int.zero;
        ground.SetTile(initialPosition, groundTile);

        return initialPosition;
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
