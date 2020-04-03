using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static UnityAction OnGameLoad;
    public static UnityAction OnGameStart;

    [Header("Player Reference")]
    public GameObject playerGameObject;
    private Player player;

    [Header("Camera Reference")]
    public GameObject cameraGameObject;
    private MovingCamera camera;

    private TerrainManager terrain;


    private void Awake()
    {
        // References to scripts
        terrain = GetComponent<TerrainManager>();
        player = playerGameObject.GetComponent<Player>();
        camera = cameraGameObject.GetComponent<MovingCamera>();

        // Generate terrain when the game loads
        OnGameLoad += terrain.Generate;

        // Add the move player method to the event called when the map has been generated
        TerrainManager.OnTerrainGenerated += StartGame;
    }

    private void Start()
    {
        OnGameLoad.Invoke();
    }


    private void StartGame()
    {
        MovePlayerToInitialTile(terrain.initialTile);
        camera.direction = MovingCamera.Direction.Right;
    }

    private void MovePlayerToInitialTile(Vector2Int initialTile)
    {
        // Move the player to the initial tile
        Vector3 initialWorldPosition = terrain.ground.CellToWorld(new Vector3Int(initialTile.x, initialTile.y, 0));

        // Get the position of the top, centre of the tile
        initialWorldPosition.x += terrain.ground.cellSize.x / 2;
        initialWorldPosition.y += terrain.ground.cellSize.y;
        // Move the player to that position
        player.SetPosition(new Vector2(initialWorldPosition.x, initialWorldPosition.y));
    }




}
