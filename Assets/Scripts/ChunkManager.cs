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


    public void GenerateNewChunk(Grid grid, SampleTerrain terrain, Vector2Int entryPositionWorld, Vector2Int chunkID)
    {
        // Create a new chunk game object 
        // This is used for the player, camera path etc
        GameObject g = Instantiate(chunkPrefab);
        g.transform.parent = transform;
        Chunk c = g.GetComponent<Chunk>();

        // Calculate some important values
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
        Vector3 enteranceWorld = grid.CellToWorld(new Vector3Int(entryPositionWorld.x, entryPositionWorld.y, 0)) + (grid.cellSize / 2);


        List<Chunk.ChunkExit> exits = new List<Chunk.ChunkExit>();
        foreach (SampleTerrain.SampleTerrainExit terrainExit in terrain.exitTilePositions)
        {
            // Calculate values
            Vector2Int exit = entryPositionWorld + terrainExit.exitPositionRelative;
            Vector3 exitPositionWorld = grid.CellToWorld(new Vector3Int(exit.x, exit.y, 0)) + (grid.cellSize / 2);

            Vector2Int newChunkTile = Vector2Int.zero;
            Vector2Int newChunkID = chunkID;

            // Move by 1 tile in correct direction
            if (terrainExit.exitDirection.Equals(SampleTerrain.ExitDirection.Up))
            {
                newChunkTile = new Vector2Int(exit.x, exit.y - 1);
                newChunkID.y += 1;
            }
            else if (terrainExit.exitDirection.Equals(SampleTerrain.ExitDirection.Down))
            {
                newChunkTile = new Vector2Int(exit.x, exit.y + 1);
                newChunkID.y -= 1;
            }
            // Horizontal case
            else if (terrainExit.exitDirection.Equals(SampleTerrain.ExitDirection.Horizontal))
            {
                if (terrain.direction.Equals(TerrainManager.TerrainDirection.Left))
                {
                    newChunkTile = new Vector2Int(exit.x - 1, exit.y);
                    newChunkID.x -= 1;
                }
                else if (terrain.direction.Equals(TerrainManager.TerrainDirection.Right))
                {
                    newChunkTile = new Vector2Int(exit.x + 1, exit.y);
                    newChunkID.x += 1;
                }
            }


            Vector3 newChunkPositionWorld = grid.CellToWorld(new Vector3Int(newChunkTile.x, newChunkTile.y, 0));

            // Create the new exit
            exits.Add(new Chunk.ChunkExit(terrainExit.exitDirection, exitPositionWorld, newChunkPositionWorld, newChunkID));
        }



        // Create the chunk
        c.CreateChunk(bounds, grid.cellSize, centre, enteranceWorld, exits, terrain.direction, chunkID);
        chunks.Add(chunkID, c);

        // Add the new camera point
        if (terrain.direction.Equals(TerrainManager.TerrainDirection.Left))
        {
            cameraPath.AddPointLeft(c.cameraPathStartWorldSpace);
        }
        else if (terrain.direction.Equals(TerrainManager.TerrainDirection.Right))
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
