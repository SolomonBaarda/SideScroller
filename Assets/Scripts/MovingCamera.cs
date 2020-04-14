using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class MovingCamera : MonoBehaviour
{
    public Chunk currentChunk;

    [Header("Movement Settings")]
    public float zoom = 7;
    public float speed = 4;
    public Direction direction = Direction.Stationary;

    [Header("GameObject to Follow")]
    public GameObject following;

    [Header("Camera Path Manager Reference")]
    public GameObject cameraPathManagerObject;
    [HideInInspector]
    public CameraPathManager path;
    private float distanceFromOrigin;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;
        distanceFromOrigin = 0;

        path = cameraPathManagerObject.GetComponent<CameraPathManager>();
    }


    public void UpdateCurrentChunk(Chunk currentChunk)
    {
        this.currentChunk = currentChunk;
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
                position = path.GetClosestPoint(following.transform.position);
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
                Vector2 length = path.GetPathLength();
                distanceFromOrigin = Mathf.Clamp(distanceFromOrigin, -length.x, length.y);

                // Get the position and zoom it out
                position = path.GetPointAtDistance(distanceFromOrigin);
            }

            position.z = -zoom;

            // Update position
            transform.position = position;
        }
    }


    public enum Direction
    {
        Left,
        Right,
        Stationary,
        Following
    }
}
