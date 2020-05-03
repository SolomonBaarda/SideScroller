using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PathCreation;
using System.Linq;

public class Chunk : MonoBehaviour
{
    public const string CHUNK_LAYER = "Chunk";

    [Header("Chunk Information")]
    public Vector2 enteranceWorldSpace;
    public List<TerrainManager.TerrainChunk.Exit> exits;

    public TerrainManager.Direction direction;
    public Vector2 bounds;
    private Vector2 cellSize;
    public Vector2Int chunkID;
    public int sampleTerrainIndex;

    [Header("Camera Path Prefab Reference")]
    public GameObject cameraPathPrefab;

    public List<CameraPath> cameraPaths;
    private Transform cameraPathChild;

    public void CreateChunk(Vector2 bounds, Vector2 cellSize, Vector2 centre, Vector2 enteranceWorldSpace,
        List<TerrainManager.TerrainChunk.Exit> exits, TerrainManager.Direction direction, int sampleTerrainIndex, Vector2Int chunkID)
    {
        // Assign variables 
        this.bounds = bounds;
        this.cellSize = cellSize;
        this.enteranceWorldSpace = enteranceWorldSpace;
        this.exits = exits;
        this.direction = direction;
        this.sampleTerrainIndex = sampleTerrainIndex;
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


                LinkedList<Vector2> l = new LinkedList<Vector2>(exit.cameraPathPoints);
                bool isFlat = l.Count <= 2;

                // Move the first and last point to exactly the edge of the chunk
                // Get the first point
                Vector2 pos = l.First.Value;
                l.RemoveFirst();
                Vector2 extra = new Vector2(pos.x, pos.y);
                switch (direction)
                {
                    case TerrainManager.Direction.Left:
                        pos.x += cellSize.x / 2;
                        extra.x = pos.x + cellSize.x;
                        break;
                    case TerrainManager.Direction.Right:
                        pos.x -= cellSize.x / 2;
                        extra.x = pos.x - cellSize.x;
                        break;
                    case TerrainManager.Direction.Up:
                        pos.y += cellSize.y / 2;
                        extra.y = pos.y + cellSize.y;
                        break;
                    case TerrainManager.Direction.Down:
                        pos.y -= cellSize.y / 2;
                        extra.y = pos.y - cellSize.y;
                        break;
                }
                l.AddFirst(pos);
                l.AddFirst(extra);

                // Get the last point
                pos = l.Last.Value;
                l.RemoveLast();
                extra = new Vector2(pos.x, pos.y);
                switch (exit.exitDirection)
                {
                    case TerrainManager.Direction.Left:
                        pos.x -= cellSize.x / 2;
                        extra.x = pos.x - cellSize.x;
                        break;
                    case TerrainManager.Direction.Right:
                        pos.x += cellSize.x / 2;
                        extra.x = pos.x + cellSize.x;
                        break;
                    case TerrainManager.Direction.Up:
                        pos.y += cellSize.y / 2;
                        extra.y = pos.y + cellSize.y;
                        break;
                    case TerrainManager.Direction.Down:
                        pos.y -= cellSize.y / 2;
                        extra.y = pos.y - cellSize.y;
                        break;
                }
                l.AddLast(pos);
                l.AddLast(extra);


                // Set the path and add it
                path.SetPath(RemoveMiddlePointsIfStraightPath(l.ToArray(), direction, exit.exitDirection), exit.newChunkID, exit.exitDirection);
                cameraPaths.Add(path);
            }
            else
            {
                Debug.LogError("Chunk " + this + " exit " + exit.exitDirection + " camera path does not contain enough points." + exit.cameraPathPoints.Count);
            }

        }
    }



    private Vector2[] RemoveMiddlePointsIfStraightPath(Vector2[] points, TerrainManager.Direction direction, TerrainManager.Direction exitDirection)
    {
        Vector2 first = points[0], last = points[points.Length - 2];

        // Horizontal case
        if ((direction == TerrainManager.Direction.Both ||
            direction == TerrainManager.Direction.Left || direction == TerrainManager.Direction.Right) &&
            (exitDirection == TerrainManager.Direction.Left || exitDirection == TerrainManager.Direction.Right))
        {
            if (first.y == last.y)
            {
                return new[] { first, last };
            }
        }

        // Vertical case
        if ((direction == TerrainManager.Direction.Up || direction == TerrainManager.Direction.Down) &&
            (exitDirection == TerrainManager.Direction.Up || exitDirection == TerrainManager.Direction.Down))
        {
            if (first.x == last.x)
            {
                return new[] { first, last };
            }
        }

        return points;
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









    // Chunk location mathods
    public static Chunk CalculateCurrentChunk(Vector2 position)
    {
        // Find the chunk at the centre point
        Collider2D collision = Physics2D.OverlapPoint(position, LayerMask.GetMask(CHUNK_LAYER));
        if (collision != null)
        {
            return collision.gameObject.GetComponent<Chunk>();
        }

        return null;
    }


    public static Chunk UpdateCurrentChunk(Chunk current, Vector2 position)
    {
        // Get the chunk the player is in now
        Chunk currentNew = CalculateCurrentChunk(position);
        if (currentNew != null)
        {
            // New chunk
            if (currentNew != current)
            {
                return currentNew;
            }
        }
        // Old chunk
        return current;
    }







}
