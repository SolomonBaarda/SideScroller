using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
            // Set position if following 
            if (direction.Equals(Direction.Following))
            {
                Vector3 position = transform.position;
                position.x = following.transform.position.x;

                // Check its not null
                if (nextChunk != null)
                {
                    //position.y = Vector3.


                }

                transform.position = position;
            } 
            // Move towards if moving in a direction
            else
            {
                Vector3 destination = transform.position;
                float distance = 0;

                // Set the correct destination
                if (direction.Equals(Direction.Left))
                {
                    destination = currentChunk.cameraPathStartWorldSpace;
                    distance = -speed;
                }
                else if (direction.Equals(Direction.Right))
                {
                    destination = nextChunk.cameraPathStartWorldSpace;
                    distance = speed;
                }

                // Move
                MoveTowards(destination, distance);
            }
        }
    }

    private void MoveTowards(Vector3 destination, float magnitude)
    {
        if (destination != null)
        {
            Vector3 distance = Vector3.ClampMagnitude(destination, magnitude * Time.deltaTime);
            transform.Translate(distance);
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
