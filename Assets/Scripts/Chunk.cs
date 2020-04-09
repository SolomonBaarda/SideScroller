using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector2Int boundsInTiles;
    public Vector2Int enteranceWorldSpace;
    public Vector2Int exitWorldSpace;

    public Vector2 bounds;

    public void CreateChunk(Vector2 bounds, Vector3 centre)
    {
        this.bounds = bounds;

        BoxCollider2D b = GetComponent<BoxCollider2D>();
        b.size = bounds;
        transform.position = centre;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("colliding with layer " + LayerMask.LayerToName(collision.gameObject.layer));
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D b = GetComponent<BoxCollider2D>();

        Gizmos.color = Color.red;

        Vector3 i = new Vector3(b.bounds.max.x, b.bounds.min.y, b.bounds.center.z);
        Vector3 j = new Vector3(b.bounds.min.x, b.bounds.max.y, b.bounds.center.z);

        // Draw a red border to the collision area
        Gizmos.DrawLine(b.bounds.min, i);
        Gizmos.DrawLine(b.bounds.min, j);
        Gizmos.DrawLine(b.bounds.max, i);
        Gizmos.DrawLine(b.bounds.max, j);
    }
}
