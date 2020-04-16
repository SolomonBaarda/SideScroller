using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PathCreation;

public class Chunk : MonoBehaviour
{
    [Header("Chunk Information")]
    public Vector3 enteranceWorldSpace;
    public List<ChunkExit> exits;

    public TerrainManager.TerrainDirection direction;
    public Vector2 bounds;
    private Vector3 cellSize;
    public Vector2Int chunkID;
    public float distanceFromOrigin;

    [Header("Camera Path Prefab Reference")]
    public GameObject cameraPathPrefab;

    public List<CameraPath> cameraPaths;

    private Transform cameraPathChild;
    private Transform itemChild;
    private const float CAMERA_POINT_OFFSET_TILES = 3f;

    public void CreateChunk(Vector2 bounds, Vector3 cellSize, Vector3 centre, Vector3 enteranceWorldSpace,
        List<ChunkExit> exits, TerrainManager.TerrainDirection direction, float distanceFromOrigin, Vector2Int chunkID)
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
        InitialiseCameraPaths();
    }



    public float CalculateNewChunkDistanceFromOrigin(CameraPath chosen)
    {
        return distanceFromOrigin + chosen.GetPathLength();
    }


    private void InitialiseCameraPaths()
    {
        float offset = CAMERA_POINT_OFFSET_TILES * cellSize.y;

        // Find the start position
        Vector3 cameraPathStartWorldSpace = enteranceWorldSpace;
        // Add a little to centre it 
        switch (direction)
        {
            case TerrainManager.TerrainDirection.Left:
                cameraPathStartWorldSpace.x += cellSize.x / 2;
                break;
            case TerrainManager.TerrainDirection.Right:
                cameraPathStartWorldSpace.x -= cellSize.x / 2;
                break;
            case TerrainManager.TerrainDirection.Both:
                // Do nothing as already in centre
                break;
        }
        cameraPathStartWorldSpace.y += offset;


        // Loop through each exit
        foreach (ChunkExit exit in exits)
        {
            // Instantiate the new object
            GameObject pathObject = Instantiate(cameraPathPrefab, cameraPathChild);
            pathObject.name = "Path " + (pathObject.transform.GetSiblingIndex() + 1) + "/" + exits.Count;
            CameraPath path = pathObject.GetComponent<CameraPath>();

            // Find its end position
            Vector3 cameraPathEndWorldSpace = exit.exitPositionWorld;
            cameraPathEndWorldSpace.y += offset;


            // Add a little to make the transition smoother
            switch (exit.exitDirection)
            {
                case (SampleTerrain.ExitDirection.Up):
                    cameraPathEndWorldSpace.y += cellSize.y / 2;
                    break;
                case (SampleTerrain.ExitDirection.Down):
                    cameraPathEndWorldSpace.y -= cellSize.y / 2;
                    break;
                case (SampleTerrain.ExitDirection.Horizontal):
                    switch (direction)
                    {
                        case (TerrainManager.TerrainDirection.Left):
                            cameraPathEndWorldSpace.x -= cellSize.x / 2;
                            break;
                        case (TerrainManager.TerrainDirection.Right):
                            cameraPathEndWorldSpace.x += cellSize.x / 2;
                            break;
                        case (TerrainManager.TerrainDirection.Both):
                            if (cameraPathEndWorldSpace.x > cameraPathStartWorldSpace.x)
                            {
                                cameraPathEndWorldSpace.x += cellSize.x / 2;

                                // Temp fix
                                cameraPathStartWorldSpace.x = 4 * cellSize.x + (cellSize.x);
                            }
                            else if (cameraPathEndWorldSpace.x < cameraPathStartWorldSpace.x)
                            {
                                cameraPathEndWorldSpace.x -= cellSize.x / 2;

                                // Temp fix
                                cameraPathStartWorldSpace.x = -4 * cellSize.x;
                            }
                            break;
                    }
                    break;
            }


            // Set the path and add it
            path.SetPath(cameraPathStartWorldSpace, cameraPathEndWorldSpace);
            cameraPaths.Add(path);
        }
    }



    private void OnTriggerStay2D(Collider2D collision)
    {
        // Player has entered the chunk
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
        {
            Vector2Int playerChunk = collision.transform.root.GetComponent<Player>().GetCurrentChunk();

            // Entered for the first time 
            if (playerChunk.x != chunkID.x || playerChunk.y != chunkID.y)
            {
                ChunkManager.OnPlayerEnterChunk.Invoke(chunkID);
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

        Vector3 i = new Vector3(b.bounds.max.x, b.bounds.min.y, b.bounds.center.z);
        Vector3 j = new Vector3(b.bounds.min.x, b.bounds.max.y, b.bounds.center.z);

        // Draw a red border to the collision area
        Gizmos.color = Color.red;
        Gizmos.DrawLine(b.bounds.min, i);
        Gizmos.DrawLine(b.bounds.min, j);
        Gizmos.DrawLine(b.bounds.max, i);
        Gizmos.DrawLine(b.bounds.max, j);

        // Centre
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, 0.5f * Vector3.one);

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

    }



    public override string ToString()
    {
        return "(" + chunkID.x + "," + chunkID.y + ")";
    }


    public class ChunkExit
    {
        public SampleTerrain.ExitDirection exitDirection;
        public Vector3 exitPositionWorld;

        public Vector3 newChunkPositionWorld;
        public TerrainManager.TerrainDirection newChunkDirection;
        public Vector2Int newChunkID;

        public ChunkExit(SampleTerrain.ExitDirection exitDirection, Vector3 exitPositionWorld, Vector3 newChunkPositionWorld,
                TerrainManager.TerrainDirection newChunkDirection, Vector2Int newChunkID)
        {
            this.exitDirection = exitDirection;
            this.exitPositionWorld = exitPositionWorld;
            this.newChunkPositionWorld = newChunkPositionWorld;
            this.newChunkDirection = newChunkDirection;
            this.newChunkID = newChunkID;
        }
    }


}
