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

    private float distanceFromOrigin;

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
                float distance = 0;

                if (direction.Equals(Direction.Left))
                {
                    distance = -speed;
                }
                else if (direction.Equals(Direction.Right))
                {
                    distance = speed;
                }

                // Update distance
                distanceFromOrigin += distance * Time.deltaTime;
                //Vector2 length = path.GetPathLength();
                //distanceFromOrigin = Mathf.Clamp(distanceFromOrigin, -length.x, length.y);

                // Get the position and zoom it out
                //position = path.GetPointAtDistance(distanceFromOrigin);
            }

            position.z = -zoom;

            // Update position
            transform.position = position;
        }
    }




    public static Vector3 GetClosestPoint(Vector3 position, Chunk chunk)
    {
        CameraPath[] paths = chunk.cameraPaths.ToArray();
        Vector3 point = paths[0].GetClosestPosition(position);

        // Loop through each path
        foreach (CameraPath path in paths)
        {
            float posToPoint = Vector3.Distance(position, point);
            Vector3 newPoint = path.GetClosestPosition(position);
            float posToNewPoint = Vector3.Distance(position, newPoint);

            // Find the closest point of all the paths
            if (posToNewPoint < posToPoint)
            {
                point = newPoint;
            }
        }

        return point;
    }


    public enum Direction
    {
        Left,
        Right,
        Stationary,
        Following
    }
}
