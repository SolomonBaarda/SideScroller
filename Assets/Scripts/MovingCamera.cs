using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class MovingCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float zoom = 10;
    public float speed = 1;
    public Direction direction = Direction.Stationary;

    [Header("GameObject to Follow")]
    public GameObject following;

    //private Chunk lastChunk;
    public Chunk currentChunk;
    public Chunk nextChunk;

    public PathCreator pathCreator;
    private float distanceTravelled;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;

    }


    public void UpdateCurrentChunk(Chunk currentChunk)
    {
        this.currentChunk = currentChunk;
    }

    public void UpdateNextChunk(Chunk nextChunk)
    {
        this.nextChunk = nextChunk;
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
                position = pathCreator.path.GetClosestPointOnPath(following.transform.position);
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
                distanceTravelled += distance * Time.deltaTime;
                distanceTravelled = Mathf.Clamp(distanceTravelled, 0, pathCreator.path.length);

                // Get the position and zoom it out
                position = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
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
