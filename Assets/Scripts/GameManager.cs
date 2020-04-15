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
    private MovingCamera movingCamera;
    public GameObject cameraPathManagerObject;
    private CameraPathManager cameraPathManager;

    [Header("Terrain Manager Reference")]
    public GameObject terrainManagerObject;
    private TerrainManager terrainManager;

    [Header("Chunk Manager Reference")]
    public GameObject chunkManagerObject;
    private ChunkManager chunkManager;

    private bool isGameOver;
    public float gameTimeSeconds;

    private void Awake()
    {
        // References to scripts
        player = playerGameObject.GetComponent<Player>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();
        cameraPathManager = cameraPathManagerObject.GetComponent<CameraPathManager>();
        terrainManager = terrainManagerObject.GetComponent<TerrainManager>();
        chunkManager = chunkManagerObject.GetComponent<ChunkManager>();

        // Generate terrain when the game loads
        OnGameLoad += terrainManager.Initialise;

        // Add event calls 
        TerrainManager.OnTerrainGenerated += StartGame;

        //ChunkManager.OnCameraEnterChunk += NewChunkEntered;
        ChunkManager.OnPlayerEnterChunk += NewChunkEntered;

        isGameOver = true;
    }

    private void Start()
    {
        OnGameLoad.Invoke();
    }


    private void FixedUpdate()
    {
        if(!isGameOver)
        {
            gameTimeSeconds += Time.deltaTime;
        }
    }

    private void StartGame()
    {
        player.SetPosition(terrainManager.GetInitialTileWorldPositionForPlayer());
        player.controller.enabled = true;

        isGameOver = false;
    }



    private void NewChunkEntered(Vector2Int chunk)
    {
        // Get the current chunk object
        Chunk current = chunkManager.GetChunk(chunk);

        // Pass it to the camera
        movingCamera.UpdateCurrentChunk(current);


        // See if any of the neighbour chunks exists
        foreach (Chunk.ChunkExit exit in current.exits)
        {
            try
            {
                // Chunk already exists, do nothing
                Chunk neighbour = chunkManager.GetChunk(exit.newChunkID);
                //Debug.Log("Neighbour chunk already exists " + neighbour);
            }
            catch (Exception)
            {
                // Does not exist, so generate it
                terrainManager.Generate(exit.newChunkPositionWorld, exit.newChunkDirection, exit.newChunkID);
                //Debug.Log("Generating new chunk");
            }

        }


    }


}
