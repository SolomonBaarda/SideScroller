using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
    public static UnityAction<Chunk, TerrainManager.TerrainChunk> OnChunkCreated;
    public static UnityAction<Vector2Int> OnChunkDestroyed;

    [Header("Chunk Prefab Reference")]
    public GameObject chunkPrefab;

    private Dictionary<Vector2Int, Chunk> chunks;
    public static readonly Vector2Int initialChunkID = Vector2Int.zero;

    public List<Chunk> LoadedChunks { get; private set; } = new List<Chunk>();

    private void Awake()
    {
        chunks = new Dictionary<Vector2Int, Chunk>();

        // Generate a new chunk when needed 
        TerrainManager.OnTerrainChunkGenerated += GenerateNewChunk;

        OnChunkCreated += SceneLoader.EMPTY;

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
        GameObject chunkObject = Instantiate(chunkPrefab, transform);
        Chunk c = chunkObject.GetComponent<Chunk>();

        // Create the chunk
        c.CreateChunk(t.bounds, t.cellSize, t.centre, t.enteranceWorldPosition, t.exits, t.respawnPoints, t.direction, t.sampleIndex, t.chunkID);

        // Add a finish to the chunk if it needs one
        if(t.finishArea.isFinish)
        {
            c.AddFinish(t.finishArea);
        }

        // Instantiate all extra objects
        for(int i = 0; i < t.extraWorldObjects.Count; i++)
        {
            GameObject g = Instantiate(t.extraWorldObjects[i].Item1, t.extraWorldObjects[i].Item2, t.extraWorldObjects[i].Item1.transform.rotation, chunkObject.transform);
            g.transform.localScale = t.extraWorldObjects[i].Item3;
        }
        c.SetNotActive();

        if(chunks.ContainsKey(t.chunkID))
        {
            chunks.Remove(t.chunkID);
        }
        // Add the new chunk
        chunks.Add(t.chunkID, c);

        // Invoke the event
        OnChunkCreated.Invoke(c, t);
    }


    public void UpdateLoadedChunks(List<Chunk> nearby)
    {
        // Disable all chunks
        foreach(Chunk c in chunks.Values)
        {
            c.SetNotActive();
        }

        // Set to be the new chunks and set active
        LoadedChunks = nearby;
        foreach(Chunk c in LoadedChunks)
        {
            c.SetActive();
        }
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
        if (!chunks.TryGetValue(chunkID, out Chunk chunk))
        {
            throw new Exception("Chunk (" + chunkID.x + "," + chunkID.y + ") could not be found.");
        }

        return chunk;
    }


}
