using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class MovingCamera : MonoBehaviour
{
    public Chunk currentChunk;

    [Header("Movement Settings")]
    public float zoom = 8;
    public float speed = 4;
    public Direction direction = Direction.Stationary;

    [Header("GameObject to Follow")]
    public GameObject following;
    private Player player;

    private CircleCollider2D centreCollision;
    private Rigidbody2D centreRigid;

    private Camera c;
    public static string cameraLayer = "Camera";
    private Vector2 cameraSize;
    private Vector2 cameraPos;

    public float distanceFromOrigin;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;

        Transform centre = transform.Find("Centre");
        centreCollision = centre.GetComponent<CircleCollider2D>();
        centreRigid = centre.GetComponent<Rigidbody2D>();
        centreRigid.isKinematic = true;

        c = GetComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = zoom;

        float height = 2f * c.orthographicSize;
        float width = height * c.aspect;
        cameraSize = new Vector2(width, height);
        cameraPos = new Vector2(transform.position.x - (cameraSize.x / 2), transform.position.y - (cameraSize.y / 2));

        try
        {
            player = following.GetComponent<Player>();
        }
        catch (System.Exception)
        {
        }

        // Update the colliders position
        Vector3 colliderPos = transform.position;
        colliderPos.z = 0;
        centreCollision.transform.position = colliderPos;

        distanceFromOrigin = 0;
    }


    public void UpdateCurrentChunk(Chunk currentChunk)
    {
        this.currentChunk = currentChunk;
    }

    public Vector2Int GetCurrentChunk()
    {
        return currentChunk.chunkID;
    }



    public List<Collider2D> GetAllNearbyChunks(Vector2 pos, Vector2 size)
    {
        List<Collider2D> collisions = new List<Collider2D>(Physics2D.OverlapBoxAll(pos, size, 0, LayerMask.GetMask("Chunk")));

        Debug.Log(collisions.Count + " collisions ");
        return collisions;
    }

    private void OnDrawGizmos()
    {
        Vector2 pos = new Vector2(transform.position.x - (cameraSize.x / 2), transform.position.y - (cameraSize.y / 2));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, new Vector2(pos.x + cameraSize.x, pos.y));
        Gizmos.DrawLine(pos, new Vector2(pos.x, pos.y + cameraSize.y));

    }


    private void FixedUpdate()
    {
        // Values used in physics2D calculations
        float height = 2f * c.orthographicSize;
        float width = height * c.aspect;
        cameraSize = new Vector2(width, height);
        cameraPos = new Vector2(transform.position.x - (cameraSize.x / 2), transform.position.y - (cameraSize.y / 2));

        if (currentChunk != null)
        {
            Move();
        }

        GetComponent<Camera>().orthographicSize = zoom;

        GetAllNearbyChunks(cameraPos, cameraSize);
    }


    private float GetDistanceFromOrigin(Vector3 point, Chunk chunk)
    {
        return chunk.distanceFromOrigin + GetClosestCameraPath(point, chunk).GetLengthAtPoint(point);
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
                // Get the chunk closest to the player if we can
                Chunk c = currentChunk;
                if (player != null)
                {
                    c = player.currentChunk;
                }
                // Update the position and update the distance along the path
                position = GetClosestPoint(following.transform.position, c);
                distanceFromOrigin = GetDistanceFromOrigin(transform.position, currentChunk);

            }
            // Move along terrain camera path
            else if (direction.Equals(Direction.Terrain))
            {
                // Update distance
                distanceFromOrigin += speed * Time.deltaTime;

                // Get the local distance
                float distanceInChunk = distanceFromOrigin - currentChunk.distanceFromOrigin;

                // Get the position in the chunk
                CameraPath path = GetClosestCameraPath(following.transform.position, currentChunk);

                // Clamp the value
                distanceInChunk = Mathf.Clamp(distanceInChunk, 0, path.GetPathLength());

                // Get the position in the chunk
                position = path.GetPositionAtDistance(distanceInChunk);
            }

            // Force zoom out
            position.z = -zoom;

            // Update position
            transform.position = position;

            // Update the colliders position
            Vector3 colliderPos = transform.position;
            colliderPos.z = 0;
            centreCollision.transform.position = colliderPos;
        }
    }


    public static CameraPath GetClosestCameraPath(Vector3 position, Chunk chunk)
    {
        // Get array of camera paths
        CameraPath[] paths = chunk.cameraPaths.ToArray();
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


    public static Vector3 GetClosestPoint(Vector3 position, Chunk chunk)
    {
        return GetClosestCameraPath(position, chunk).GetClosestPosition(position);
    }


    public enum Direction
    {
        Terrain,
        Stationary,
        Following
    }
}
