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

    private TerrainManager terrainManager;

    private void Awake()
    {
        // References to scripts
        terrainManager = GetComponent<TerrainManager>();
        player = playerGameObject.GetComponent<Player>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();

        // Generate terrain when the game loads
        OnGameLoad += terrainManager.Generate;

        // Add the move player method to the event called when the map has been generated
        TerrainManager.OnTerrainGenerated += StartGame;
    }

    private void Start()
    {
        OnGameLoad.Invoke();
    }


    private void StartGame()
    {
        player.SetPosition(terrainManager.GetInitialTileWorldPositionForPlayer());
    }

    private void FixedUpdate()
    {
        Chunk chunk;
        terrainManager.chunks.TryGetValue(terrainManager.GetNextChunk(), out chunk);
        Vector3 nextChunkCameraPoint = chunk.cameraPathStartWorldSpace;

        movingCamera.Move(nextChunkCameraPoint);
    }





}
