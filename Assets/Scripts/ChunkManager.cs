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
    public Vector2Int lastGeneratedChunk;

    private CameraPath cameraPath;

    private void Awake()
    {
        cameraPath = GetComponentInChildren<CameraPath>();

        chunks = new Dictionary<Vector2Int, Chunk>();

        // Generate a new chunk when needed 
        TerrainManager.OnTerrainChunkGenerated += GenerateNewChunk;
    }


    public void GenerateNewChunk(Grid grid, SampleTerrain terrain, Vector2Int entryPositionWorld)
    {
        // Create a new chunk game object 
        // This is used for the player, camera path etc
        GameObject g = Instantiate(chunkPrefab);
        g.transform.parent = transform;
        Chunk c = g.GetComponent<Chunk>();


        Vector2 bounds = terrain.GetGroundBounds();
        Vector3 centre = grid.CellToWorld(new Vector3Int((int)(entryPositionWorld.x + (bounds.x / 2)), (int)(entryPositionWorld.y + (bounds.y / 2)), 0));
        Vector3 tileSize = terrain.GetTileSize();

        // Need to add half a cell for odd numbers as it was casted to int
        if (bounds.x % 2 == 1)
        {
            centre.x += tileSize.x / 2;
        }
        if (bounds.y % 2 == 1)
        {
            centre.y += tileSize.y / 2;
        }

        // Add half a cell to centre the position for both
        Vector3 enteranceWorld = grid.CellToWorld(new Vector3Int(entryPositionWorld.x, entryPositionWorld.y, 0)) + new Vector3(tileSize.x / 2, tileSize.y / 2, 0);
        List<Vector3> exitWorldPositions = new List<Vector3>();
        foreach(Vector2Int pos in terrain.exitTilePositions)
        {
            Vector2Int exit = entryPositionWorld + pos;
            exitWorldPositions.Add(grid.CellToWorld(new Vector3Int(exit.x, exit.y, 0)) + new Vector3(tileSize.x / 2, tileSize.y / 2, 0));
        }

        // Assign the direction
        int direction = 0;
        if(terrain.direction.Equals(TerrainManager.TerrainDirection.Left))
        {
            direction = -1;
        }
        else if (terrain.direction.Equals(TerrainManager.TerrainDirection.Right))
        {
            direction = 1;
        }
        if (terrain.direction.Equals(TerrainManager.TerrainDirection.Undefined))
        {
            throw new Exception("Terrain direction is undefined in chunk generation.");
        }
        lastGeneratedChunk.x += direction;

        // Create the chunk
        c.CreateChunk(bounds, grid.cellSize, centre, enteranceWorld, exitWorldPositions, lastGeneratedChunk);
        chunks.Add(lastGeneratedChunk, c);


        cameraPath.AddPoint(c.cameraPathStartWorldSpace);
    }


 

    public Chunk GetChunk(Vector2Int chunkID)
    {
        Chunk chunk;
        if(!chunks.TryGetValue(chunkID, out chunk))
        {
            throw new Exception("Chunk (" + chunkID.x + "," + chunkID.y +") could not be found.");
        }

        return chunk;
    }



}
