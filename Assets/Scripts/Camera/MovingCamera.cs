﻿using System.Collections.Generic;
using UnityEngine;

public class MovingCamera : MonoBehaviour, ILocatable
{
    [Header("Movement Settings")]
    public float zoom = 16;
    public float speed = 4;
    public Direction direction = Direction.Stationary;

    private ILocatable following;

    private Camera c;
    public static string LAYER = "Camera";

    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }
    public Payload.Direction IdealDirection
    {
        get { return Payload.Direction.None; }
    }

    public Bounds ViewBounds
    {
        get
        {
            Vector2 bl = c.ViewportToWorldPoint(new Vector3(0, 0, zoom));
            Vector2 tr = c.ViewportToWorldPoint(new Vector3(1, 1, zoom));
            return new Bounds((Vector2)transform.position, tr - bl);
        }
    }

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;

        // Camera settings
        c = GetComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = zoom;
    }


    private void Update()
    {
        UpdateCurrentChunk();

        if (CurrentChunk != null)
        {
            Move();
        }

        GetComponent<Camera>().orthographicSize = zoom;
    }


    public bool SetFollowingTarget(GameObject toFollow)
    {
        // Set the target
        if (WorldItem.ExtendsClass<ILocatable>(toFollow))
        {
            following = (ILocatable)WorldItem.GetClass<ILocatable>(toFollow);
            return true;
        }
        return false;
    }


    public List<Chunk> GetAllNearbyChunks()
    {
        return GetAllNearbyChunks(ViewBounds.min, ViewBounds.max);
    }


    public static List<Chunk> GetAllNearbyChunks(Vector2 bottomLeft, Vector2 topRight)
    {
        Vector2 size = topRight - bottomLeft;
        // Get a list of colliders
        Collider2D[] collisions = Physics2D.OverlapBoxAll(bottomLeft + (size / 2), size, 0, LayerMask.GetMask(Chunk.CHUNK_LAYER));
        List<Chunk> chunks = new List<Chunk>();

        // Get each chunk from them
        foreach (Collider2D c in collisions)
        {
            Chunk chunk = c.gameObject.GetComponent<Chunk>();
            if (!chunks.Contains(chunk))
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }



    private void Move()
    {
        // Don't move if stationary
        if (!direction.Equals(Direction.Stationary))
        {
            Vector3 position = transform.position;

            // Set position if following 
            if (direction.Equals(Direction.Following))
            {                
                if (following != null)
                {
                    Chunk c = CurrentChunk;

                    // Get the chunk closest to the player if we can
                    if (following.CurrentChunk != null)
                    {
                        c = following.CurrentChunk;
                    }

                    // Get the closest point in the chunk to the target
                    Vector3 possiblePosition = GetClosestPoint(following.Position, c);

                    // Ensure the new position is in the correct direction for the object following
                    if (following.IdealDirection == Payload.Direction.None ||
                        (following.IdealDirection == Payload.Direction.Left && possiblePosition.x < transform.position.x) ||
                        (following.IdealDirection == Payload.Direction.Right && possiblePosition.x > transform.position.x))
                    {
                        position = possiblePosition;
                    }
                }
            }
            // Move along terrain camera path
            else if (direction.Equals(Direction.Terrain))
            {
                Vector2 newPos = transform.position;
                // The path that the player is following
                CameraPath p = GetClosestCameraPath(following.Position, CurrentChunk);
                float distance = speed * Time.deltaTime;

                // Move in the correct direction
                switch (p.pathDirection)
                {
                    case TerrainManager.Direction.Left:
                        newPos.x -= distance;
                        break;
                    case TerrainManager.Direction.Right:
                        newPos.x += distance;
                        break;
                    case TerrainManager.Direction.Up:
                        newPos.y += distance;
                        break;
                    case TerrainManager.Direction.Down:
                        newPos.y -= distance;
                        break;
                }

                // Get the closest position
                position = p.GetClosestPosition(newPos);
            }


            // Force zoom out
            position.z = -zoom;

            // Update position
            transform.position = position;
        }
    }


    public static CameraPath GetClosestCameraPath(Vector3 position, Chunk chunk)
    {
        if (chunk != null)
        {
            // Get array of camera paths
            if (chunk.cameraPaths.Count> 0)
            {
                // Get first point
                int index = 0;
                Vector3 point = chunk.cameraPaths[index].GetClosestPosition(position);

                // Loop through each other path
                for (int i = 1; i < chunk.cameraPaths.Count; i++)
                {
                    // Calculate the distance
                    float posToPoint = Vector3.Distance(position, point);

                    // Calculate current new point
                    Vector3 newPoint = chunk.cameraPaths[i].GetClosestPosition(position);
                    float posToNewPoint = Vector3.Distance(position, newPoint);

                    // Find the closest point of the two
                    if (posToNewPoint < posToPoint)
                    {
                        // Update it and the index
                        point = newPoint;
                        index = i;
                    }
                }

                return chunk.cameraPaths[index];
            }
        }

        throw new System.Exception("Cannot calculate camera path for null chunk or null path");
    }


    public static Vector3 GetClosestPoint(Vector3 position, Chunk chunk)
    {
        return GetClosestCameraPath(position, chunk).GetClosestPosition(position);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (c != null)
        {
            // Draw the camera view 
            Vector2 bl = ViewBounds.min;
            Vector2 br = new Vector2(ViewBounds.max.x, ViewBounds.min.y);
            Vector2 tl = new Vector2(ViewBounds.min.x, ViewBounds.max.y);
            Vector2 tr = ViewBounds.max;
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(tr, tl);
        }
    }


    public void UpdateCurrentChunk()
    {
        // Only update the current chunk if we are no longer in the old one
        if(CurrentChunk == null || !CurrentChunk.Bounds.Contains(transform.position)) 
        {
            CurrentChunk = Chunk.UpdateCurrentChunk(CurrentChunk, Position);
        }
    }


    public enum Direction
    {
        Terrain,
        Stationary,
        Following
    }
}
