using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MovingCamera : MonoBehaviour, ILocatable
{
    [Header("Movement Settings")]
    public float zoom = 8;
    public float speed = 4;
    public Direction direction = Direction.Stationary;

    private ILocatable following;

    private Camera c;
    public static string LAYER_CAMERA = "Camera";

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
                Chunk c = CurrentChunk;
                Vector2 closest = transform.position;
                if (following != null)
                {
                    // Get the chunk closest to the player if we can
                    if (following.CurrentChunk != null)
                    {
                        c = following.CurrentChunk;
                    }
                    // Get the closest position if we can 
                    closest = following.Position;
                }

                // Update the position and update the distance along the path
                Vector3 possiblePosition = GetClosestPoint(closest, c);

                // Ensure the new position is in the correct direction for the object following
                if ((following.IdealDirection == Payload.Direction.Left && position.x < following.Position.x) ||
                    (following.IdealDirection == Payload.Direction.Right && position.x > following.Position.x) ||
                    following.IdealDirection == Payload.Direction.None)
                {
                    position = possiblePosition;
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
            CameraPath[] paths = chunk.cameraPaths.ToArray();

            if (paths.Length > 0)
            {
                // Get first point
                Vector3 point = paths[0].GetClosestPosition(position);
                int index = 0;

                // Loop through each other path
                for (int i = 1; i < paths.Length; i++)
                {
                    // Calculate the distance
                    float posToPoint = Vector3.Distance(position, point);
                    // Calculate current new point
                    Vector3 newPoint = paths[i].GetClosestPosition(position);
                    float posToNewPoint = Vector3.Distance(position, newPoint);

                    // Find the closest point of the two
                    if (posToNewPoint < posToPoint)
                    {
                        // Update it and the index
                        point = newPoint;
                        index = i;
                    }
                }

                return paths[index];
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
            Vector3 bl = c.ViewportToWorldPoint(new Vector3(0, 0, zoom));
            Vector3 br = c.ViewportToWorldPoint(new Vector3(1, 0, zoom));
            Vector3 tl = c.ViewportToWorldPoint(new Vector3(0, 1, zoom));
            Vector3 tr = c.ViewportToWorldPoint(new Vector3(1, 1, zoom));
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(tr, tl);
        }
    }


    public void UpdateCurrentChunk()
    {
        CurrentChunk = Chunk.UpdateCurrentChunk(CurrentChunk, Position);
    }


    public enum Direction
    {
        Terrain,
        Stationary,
        Following
    }
}
