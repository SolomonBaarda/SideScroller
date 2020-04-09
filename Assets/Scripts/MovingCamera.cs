using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovingCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float zoom = 10;
    public float speed = 1;
    public Direction direction = Direction.Stationary;

    public GameObject following;


    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 pos = transform.position;

        if (direction.Equals(Direction.Following))
        {
            pos.x = following.transform.position.x;
            pos.y = following.transform.position.y;
        }
        else if (direction.Equals(Direction.Stationary))
        {
            // Do nothing
        }
        else if (direction.Equals(Direction.Left))
        {
            pos.x -= speed * Time.deltaTime;
            pos.y = following.transform.position.y;
        }
        else if (direction.Equals(Direction.Right))
        {
            pos.x += speed * Time.deltaTime;
            pos.y = following.transform.position.y;
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
