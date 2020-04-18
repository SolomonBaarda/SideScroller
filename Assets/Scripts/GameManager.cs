using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static UnityAction OnGameStart;

    [Header("Player")]
    public GameObject playerGameObject;
    private Player player;

    [Header("Camera")]
    public GameObject cameraGameObject;
    private MovingCamera movingCamera;

    [Header("Terrain Manager")]
    public GameObject terrainManagerObject;
    private TerrainManager terrainManager;

    [Header("Chunk Manager")]
    public GameObject chunkManagerObject;
    private ChunkManager chunkManager;

    [Header("Item Manager")]
    public GameObject itemManagerObject;
    private ItemManager itemManager;

    [Header("Game Settings")]
    public float gameTimeSeconds;
    private bool isGameOver;

    private void Awake()
    {
        // References to scripts
        player = playerGameObject.GetComponent<Player>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();
        terrainManager = terrainManagerObject.GetComponent<TerrainManager>();
        chunkManager = chunkManagerObject.GetComponent<ChunkManager>();
        itemManager = itemManagerObject.GetComponent<ItemManager>();

        // Add event calls 
        TerrainManager.OnTerrainGenerated += StartGame;
        //Menu.OnMenuClose += StartGame;

        ChunkManager.OnCameraEnterChunk += CameraEnteredNewChunk;
        ChunkManager.OnPlayerEnterChunk += PlayerEnteredNewChunk;

        isGameOver = true;
    }

    private void Start()
    {
        // Generate terrain when the game loads
        terrainManager.Initialise();
    }

    private void OnDestroy()
    {
        Menu.OnMenuClose -= StartGame;
        ChunkManager.OnCameraEnterChunk -= CameraEnteredNewChunk;
        ChunkManager.OnPlayerEnterChunk -= PlayerEnteredNewChunk;
    }


    private void FixedUpdate()
    {
        if (!isGameOver)
        {
            gameTimeSeconds += Time.deltaTime;
        }

        if (Input.GetKeyDown(player.controller.keys.escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(player.controller.keys.interact2))
        {
            switch (movingCamera.direction)
            {
                case MovingCamera.Direction.Terrain:
                    movingCamera.direction = MovingCamera.Direction.Following;
                    break;
                case MovingCamera.Direction.Stationary:
                    movingCamera.direction = MovingCamera.Direction.Terrain;
                    break;
                case MovingCamera.Direction.Following:
                    movingCamera.direction = MovingCamera.Direction.Stationary;
                    break;
            }
        }
    }

    private void StartGame()
    {
        player.SetPosition(terrainManager.GetInitialTileWorldPositionForPlayer());
        player.controller.enabled = true;

        movingCamera.direction = MovingCamera.Direction.Following;

        isGameOver = false;
    }


    private void PlayerEnteredNewChunk(Vector2Int chunk)
    {
        try
        {
            Chunk c = chunkManager.GetChunk(chunk);
            player.SetCurrentChunk(c);
        }
        catch (Exception)
        {
        }
    }

    private void CameraEnteredNewChunk(Vector2Int chunk)
    {
        try
        {
            // Get the current chunk object
            Chunk current = chunkManager.GetChunk(chunk);

            // Pass it to the camera
            movingCamera.UpdateCurrentChunk(current);

            // See if any of the neighbour chunks exists
            foreach (TerrainManager.TerrainChunk.Exit exit in current.exits)
            {
                Debug.Log(exit.newChunkID);
                try
                {
                    // Chunk already exists, do nothing
                    Chunk neighbour = chunkManager.GetChunk(exit.newChunkID);
                }
                catch (Exception)
                {
                    // Does not exist, so generate it
                    // Calculate how far along the camera path the new chunk is
                    float distanceFromOrigin = current.CalculateNewChunkDistanceFromOrigin(MovingCamera.GetClosestCameraPath(exit.exitPositionWorld, current));
                    // Generate the new chunk
                    terrainManager.Generate(exit.newChunkPositionWorld, exit.exitDirection, distanceFromOrigin, exit.newChunkID);
                }
            }
        }
        catch (Exception)
        {
        }
    }



    private void CheckGenerateNew(Vector2Int chunk)
    {
        // TODO MOVE TO HERE
    }


}
