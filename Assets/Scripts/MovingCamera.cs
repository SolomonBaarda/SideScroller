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

    public float distanceFromOrigin;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;

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



    private void FixedUpdate()
    {
        Move();

        GetComponent<Camera>().orthographicSize = zoom;
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
                position = GetClosestPoint(following.transform.position, currentChunk);
            }
            else
            {
                // Update distance
                distanceFromOrigin += speed * Time.deltaTime;

                // Get the local distance
                float distanceInChunk = distanceFromOrigin - currentChunk.distanceFromOrigin;

                // Get the position in the chunk
                CameraPath path = GetClosestCameraPath(following.transform.position, currentChunk);

                //distanceInChunk = Mathf.Clamp(distanceFromOrigin, 0, path.GetPathLength());

                position = path.GetPositionAtDistance(distanceInChunk);
            }

            // Force zoom out
            position.z = -zoom;

            // Update position
            transform.position = position;
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
