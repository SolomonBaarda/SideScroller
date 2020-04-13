using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Chunk : MonoBehaviour
{
    public Vector3 enteranceWorldSpace;
    public List<ChunkExit> exits;

    public TerrainManager.TerrainDirection direction;
    public Vector2 bounds;
    public Vector2Int chunkID;

    public Vector3 cameraPathStartWorldSpace;

    public void CreateChunk(Vector2 bounds, Vector3 cellSize, Vector3 centre, Vector3 enteranceWorldSpace,
        List<ChunkExit> exits, TerrainManager.TerrainDirection direction, Vector2Int chunkID)
    {
        // Assign variables 
        this.bounds = bounds;
        this.enteranceWorldSpace = enteranceWorldSpace;
        this.exits = exits;
        this.direction = direction;
        this.chunkID = chunkID;

        // Set the name 
        transform.name = ToString();

        // Exit collider
        BoxCollider2D b = GetComponent<BoxCollider2D>();
        b.size = bounds;
        transform.position = centre;

        // Move the camera up by 2 cells
        cameraPathStartWorldSpace = enteranceWorldSpace;
        cameraPathStartWorldSpace.y += 2 * cellSize.y;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player has entered the chunk
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
        {
            //Debug.Log("Player in chunk " + transform.name);
            ChunkManager.OnPlayerEnterChunk.Invoke(chunkID);
        }



    }

    private void OnDrawGizmos()
    {
        BoxCollider2D b = GetComponent<BoxCollider2D>();

        Vector3 i = new Vector3(b.bounds.max.x, b.bounds.min.y, b.bounds.center.z);
        Vector3 j = new Vector3(b.bounds.min.x, b.bounds.max.y, b.bounds.center.z);

        // Draw a red border to the collision area
        Gizmos.color = Color.red;
        Gizmos.DrawLine(b.bounds.min, i);
        Gizmos.DrawLine(b.bounds.min, j);
        Gizmos.DrawLine(b.bounds.max, i);
        Gizmos.DrawLine(b.bounds.max, j);

        // Enterance marker
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(enteranceWorldSpace, 0.5f * Vector3.one);

        // Exit markers
        foreach (ChunkExit exit in exits)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(exit.exitPositionWorld, 0.5f * Vector3.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(exit.newChunkPositionWorld, 0.5f * Vector3.one);
        }

        // Camera point 
        Gizmos.color = Color.green;
        Gizmos.DrawCube(cameraPathStartWorldSpace, 0.5f * Vector3.one);
    }


    public class ChunkExit
    {
        public SampleTerrain.ExitDirection exitDirection;
        public Vector3 exitPositionWorld;

        public Vector3 newChunkPositionWorld;
        public Vector2Int newChunkID;

        public ChunkExit(SampleTerrain.ExitDirection exitDirection, Vector3 exitPositionWorld, Vector3 newChunkPositionWorld, Vector2Int newChunkID)
        {
            this.exitDirection = exitDirection;
            this.exitPositionWorld = exitPositionWorld;
            this.newChunkPositionWorld = newChunkPositionWorld;
            this.newChunkID = newChunkID;
        }

    }

    public override string ToString()
    {
        return "(" + chunkID.x + "," + chunkID.y + ")";
    }
}
