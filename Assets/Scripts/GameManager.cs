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

    [Header("Terrain Manager Reference")]
    public GameObject terrainManagerObject;
    private TerrainManager terrainManager;

    [Header("Chunk Manager Reference")]
    public GameObject chunkManagerObject;
    private ChunkManager chunkManager;

    private void Awake()
    {
        // References to scripts
        player = playerGameObject.GetComponent<Player>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();
        terrainManager = terrainManagerObject.GetComponent<TerrainManager>();
        chunkManager = chunkManagerObject.GetComponent<ChunkManager>();

        // Generate terrain when the game loads
        OnGameLoad += terrainManager.Initialise;

        // Add event calls 
        TerrainManager.OnTerrainGenerated += StartGame;
        ChunkManager.OnPlayerEnterChunk += NewChunkEntered;

        
    }

    private void Start()
    {
        OnGameLoad.Invoke();
    }


    private void StartGame()
    {
        player.SetPosition(terrainManager.GetInitialTileWorldPositionForPlayer());
    }





    private void NewChunkEntered(Vector2Int chunk)
    {
        // Get the current chunk object
        Chunk c = chunkManager.GetChunk(chunk);
        // Pass it to the camera
        movingCamera.UpdateCurrentChunk(c);


        if(movingCamera.currentChunk.chunkID.Equals(chunkManager.lastGeneratedChunk))
        {
            terrainManager.Generate();
        }
    }






}
