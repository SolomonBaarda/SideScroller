using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PathCreation;

public class Chunk : MonoBehaviour
{
    public static string CHUNK = "Chunk";

    [Header("Chunk Information")]
    public Vector2 enteranceWorldSpace;
    public List<TerrainManager.TerrainChunk.Exit> exits;

    public TerrainManager.TerrainDirection direction;
    public Vector2 bounds;
    private Vector2 cellSize;
    public Vector2Int chunkID;
    public float distanceFromOrigin;

    [Header("Camera Path Prefab Reference")]
    public GameObject cameraPathPrefab;

    public List<CameraPath> cameraPaths;

    private Transform cameraPathChild;
    private Transform itemChild;


    public void CreateChunk(Vector2 bounds, Vector2 cellSize, Vector2 centre, Vector2 enteranceWorldSpace,
        List<TerrainManager.TerrainChunk.Exit> exits, TerrainManager.TerrainDirection direction, float distanceFromOrigin, Vector2Int chunkID)
    {
        // Assign variables 
        this.bounds = bounds;
        this.cellSize = cellSize;
        this.enteranceWorldSpace = enteranceWorldSpace;
        this.exits = exits;
        this.direction = direction;
        this.distanceFromOrigin = distanceFromOrigin;
        this.chunkID = chunkID;

        // Set the name 
        transform.name = ToString();

        // Exit collider
        BoxCollider2D b = GetComponent<BoxCollider2D>();
        b.size = bounds;
        transform.position = centre;

        // Get the camera paths
        cameraPathChild = transform.Find("Camera Paths");
        cameraPaths = new List<CameraPath>();
        InitialiseCameraPaths(exits);
    }



    public float CalculateNewChunkDistanceFromOrigin(CameraPath chosen)
    {
        return distanceFromOrigin + chosen.GetPathLength();
    }


    private void InitialiseCameraPaths(List<TerrainManager.TerrainChunk.Exit> exits)
    {
        // Loop through each exit
        foreach (TerrainManager.TerrainChunk.Exit exit in exits)
        {
            if (exit.cameraPathPoints.Count >= 2)
            {
                // Instantiate the new object
                GameObject pathObject = Instantiate(cameraPathPrefab, cameraPathChild);
                pathObject.name = "Path " + (pathObject.transform.GetSiblingIndex() + 1) + "/" + exits.Count;
                CameraPath path = pathObject.GetComponent<CameraPath>();


                // Move the first and last point to exactly the edge of the chunk
                Vector2[] pathPoints = exit.cameraPathPoints.ToArray();
                // Get the first point
                Vector2 pos = pathPoints[0];
                switch (direction)
                {
                    case TerrainManager.TerrainDirection.Left:
                        pos.x += cellSize.x / 2;
                        break;
                    case TerrainManager.TerrainDirection.Right:
                        pos.x -= cellSize.x / 2;
                        break;
                    case TerrainManager.TerrainDirection.Up:
                        pos.y -= cellSize.y / 2;
                        break;
                    case TerrainManager.TerrainDirection.Down:
                        pos.y += cellSize.y / 2;
                        break;
                }
                pathPoints[0] = pos;
                // Get the last point
                pos = pathPoints[pathPoints.Length - 1];
                switch (exit.exitDirection)
                {
                    case TerrainManager.TerrainDirection.Left:
                        pos.x -= cellSize.x / 2;
                        break;
                    case TerrainManager.TerrainDirection.Right:
                        pos.x += cellSize.x / 2;
                        break;
                    case TerrainManager.TerrainDirection.Up:
                        pos.y += cellSize.y / 2;
                        break;
                    case TerrainManager.TerrainDirection.Down:
                        pos.y -= cellSize.y / 2;
                        break;
                }
                pathPoints[pathPoints.Length - 1] = pos;


                // Set the path and add it
                path.SetPath(pathPoints);
                cameraPaths.Add(path);
            }
            else
            {
                Debug.LogError("Chunk " + this + " exit " + exit.exitDirection + " camera path does not contain enough points." + exit.cameraPathPoints.Count);
            }

        }
    }



    private void OnDestroy()
    {
        ChunkManager.OnChunkDestroyed.Invoke(chunkID);
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D b = GetComponent<BoxCollider2D>();

        Vector2 i = new Vector2(b.bounds.max.x, b.bounds.min.y);
        Vector2 j = new Vector2(b.bounds.min.x, b.bounds.max.y);

        // Draw a red border to the collision area
        Gizmos.color = Color.red;
        Gizmos.DrawLine(b.bounds.min, i);
        Gizmos.DrawLine(b.bounds.min, j);
        Gizmos.DrawLine(b.bounds.max, i);
        Gizmos.DrawLine(b.bounds.max, j);
    }

    private void OnDrawGizmosSelected()
    {
        // Centre
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, 0.5f * Vector2.one);

        // Enterance marker
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(enteranceWorldSpace, 0.5f * Vector2.one);

        // Exit markers
        foreach (TerrainManager.TerrainChunk.Exit exit in exits)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(exit.exitPositionWorld, 0.5f * Vector2.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(exit.newChunkPositionWorld, 0.5f * Vector2.one);
        }
    }



    public override string ToString()
    {
        return "(" + chunkID.x + "," + chunkID.y + ")";
    }




}
