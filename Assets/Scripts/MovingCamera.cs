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


    public Chunk currentChunk;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;
    }




    public void UpdateCurrentChunk(Chunk chunk)
    {
        currentChunk = chunk;
    }



    public void Move(Vector3 destination)
    {
        Vector3 pos = transform.position;

        if (!direction.Equals(Direction.Stationary))
        {
            Vector3 snapshot = (destination - pos) * speed * Time.deltaTime;
            pos.y += snapshot.y;

            if (direction.Equals(Direction.Following))
            {
                pos.x = following.transform.position.x;
            }
            else if (direction.Equals(Direction.Left))
            {
                pos.x -= snapshot.x;
            }
            else if (direction.Equals(Direction.Right))
            {
                pos.x += snapshot.x;
            }
        }

        transform.position = pos;

        GetComponent<Camera>().orthographicSize = zoom;
    }

    public enum Direction
    {
        Left,
        Right,
        Stationary,
        Following
    }
}
