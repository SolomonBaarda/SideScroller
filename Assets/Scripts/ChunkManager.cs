using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
    public static UnityAction<Vector2Int> OnCameraEnterChunk;
    public static UnityAction<Vector2Int> OnPlayerEnterChunk;

    [Header("Chunk Prefab Reference")]
    public GameObject chunkPrefab;

    private Dictionary<Vector2Int, Chunk> chunks;

    private CameraPath cameraPath;

    private void Awake()
    {
        cameraPath = GetComponentInChildren<CameraPath>();

        chunks = new Dictionary<Vector2Int, Chunk>();

        // Generate a new chunk when needed 
        TerrainManager.OnTerrainChunkGenerated += GenerateNewChunk;
    }


    public void GenerateNewChunk(TerrainManager.TerrainChunk terrainChunk)
    {
        // Create a new chunk game object 
        // This is used for the player, camera path etc
        GameObject g = Instantiate(chunkPrefab);
        g.transform.parent = transform;
        Chunk c = g.GetComponent<Chunk>();

        // Create the chunk
        c.CreateChunk(terrainChunk.bounds, terrainChunk.cellSize, terrainChunk.centre, terrainChunk.enteranceWorldPosition,
                terrainChunk.exits, terrainChunk.direction, terrainChunk.chunkID);
        chunks.Add(terrainChunk.chunkID, c);



        // Add the new camera point
        if (terrainChunk.direction.Equals(TerrainManager.TerrainDirection.Left))
        {
            cameraPath.AddPointLeft(c.cameraPathStartWorldSpace);
        }
        else if (terrainChunk.direction.Equals(TerrainManager.TerrainDirection.Right))
        {
            cameraPath.AddPointRight(c.cameraPathStartWorldSpace);
        }

    }



    public Chunk GetChunk(Vector2Int chunkID)
    {
        Chunk chunk;
        if (!chunks.TryGetValue(chunkID, out chunk))
        {
            throw new Exception("Chunk (" + chunkID.x + "," + chunkID.y + ") could not be found.");
        }

        return chunk;
    }



}
