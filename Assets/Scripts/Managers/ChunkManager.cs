using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
    public static UnityAction<Vector2Int> OnChunkDestroyed;

    [Header("Chunk Prefab Reference")]
    public GameObject chunkPrefab;

    private Dictionary<Vector2Int, Chunk> chunks;
    public static readonly Vector2Int initialChunkID = Vector2Int.zero;

    private void Awake()
    {
        chunks = new Dictionary<Vector2Int, Chunk>();

        // Generate a new chunk when needed 
        TerrainManager.OnTerrainChunkGenerated += GenerateNewChunk;

        OnChunkDestroyed += RemoveChunk;
    }

    private void OnDestroy()
    {
        TerrainManager.OnTerrainChunkGenerated -= GenerateNewChunk;

        OnChunkDestroyed -= RemoveChunk;
    }

    private void GenerateNewChunk(TerrainManager.TerrainChunk t)
    {
        // Create a new chunk game object 
        // This is used for the player, camera path etc
        GameObject g = Instantiate(chunkPrefab, transform);
        Chunk c = g.GetComponent<Chunk>();

        // Create the chunk
        c.CreateChunk(t.bounds, t.cellSize, t.centre, t.enteranceWorldPosition, t.exits, t.respawnPoints, t.direction, t.sampleIndex, t.chunkID);
        if(chunks.ContainsKey(t.chunkID))
        {
            chunks.Remove(t.chunkID);
        }
        // Add the new chunk
        chunks.Add(t.chunkID, c);
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
