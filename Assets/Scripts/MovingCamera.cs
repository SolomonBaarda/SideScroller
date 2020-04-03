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
        float distance = direction == Direction.Right ? speed : -speed;
        if (direction == Direction.Stationary)
        {
            distance = 0;
        }

        transform.Translate(new Vector3(distance * Time.deltaTime, 0, 0));

        if (following != null)
        {
            transform.position = new Vector3(transform.position.x, following.transform.position.y, transform.position.z);
        }

        GetComponent<Camera>().orthographicSize = zoom;
    }

    public enum Direction
    {
        Left,
        Right,
        Stationary
    }
}
