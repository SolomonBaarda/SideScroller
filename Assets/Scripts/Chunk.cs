using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Chunk : MonoBehaviour
{
    public Vector3 enteranceWorldSpace;
    public Vector3 exitWorldSpace;

    public Vector3 cameraPathStartWorldSpace;

    public Vector2 bounds;
    private Vector3 cellSize;

    public Vector2Int chunkID;

    public void CreateChunk(Vector2 bounds, Vector3 cellSize, Vector3 centre, Vector3 enteranceWorldSpace, Vector3 exitWorldSpace, Vector2Int chunkID)
    {
        this.bounds = bounds;
        this.cellSize = cellSize;
        this.enteranceWorldSpace = enteranceWorldSpace;
        this.exitWorldSpace = exitWorldSpace;
        this.chunkID = chunkID;
        transform.name = "(" + chunkID.x + "," + chunkID.y + ")";

        BoxCollider2D b = GetComponent<BoxCollider2D>();
        b.size = bounds;
        transform.position = centre;

        // Move the camera up by 2 cells
        cameraPathStartWorldSpace = enteranceWorldSpace;
        cameraPathStartWorldSpace.y += 2 * cellSize.y;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
        {
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
        // Exit marker
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(exitWorldSpace, 0.5f * Vector3.one);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(cameraPathStartWorldSpace, 0.5f * Vector3.one);
    }
}
