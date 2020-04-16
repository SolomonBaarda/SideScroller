﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
    public static UnityAction<Vector2Int> OnCameraEnterChunk;
    public static UnityAction<Vector2Int> OnPlayerEnterChunk;

    public static UnityAction<Vector2Int> OnChunkDestroyed;

    [Header("Chunk Prefab Reference")]
    public GameObject chunkPrefab;

    private Dictionary<Vector2Int, Chunk> chunks;

    private void Awake()
    {
        chunks = new Dictionary<Vector2Int, Chunk>();

        // Generate a new chunk when needed 
        TerrainManager.OnTerrainChunkGenerated += GenerateNewChunk;

        OnChunkDestroyed += RemoveChunk;
    }


    public void GenerateNewChunk(TerrainManager.TerrainChunk terrainChunk)
    {
        // Create a new chunk game object 
        // This is used for the player, camera path etc
        GameObject g = Instantiate(chunkPrefab, transform);
        Chunk c = g.GetComponent<Chunk>();

        // Create the chunk
        c.CreateChunk(terrainChunk.bounds, terrainChunk.cellSize, terrainChunk.centre, terrainChunk.enteranceWorldPosition,
                terrainChunk.exits, terrainChunk.direction, terrainChunk.chunkID);
        chunks.Add(terrainChunk.chunkID, c);
    }


    private void RemoveChunk(Vector2Int chunkID)
    {
        try
        {
            Chunk c = GetChunk(chunkID);
            chunks.Remove(chunkID);
        }
        catch (Exception)
        {
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