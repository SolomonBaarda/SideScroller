using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isAlive;

    public int coinCount;

    public PlayerMovement controller;

    private void Awake()
    {
        // Get reference to the controller script
        controller = GetComponent<PlayerMovement>();
        //controller.enabled = false;

        isAlive = true;
        coinCount = 0;

        GameManager.OnGameStart += SetAlive;
    }

    private void Update()
    {
        if (Input.GetKey(controller.keys.slow))
        {
            SetAlive();
        }
    }


    public void SetPosition(Vector2 position)
    {
        BoxCollider2D b = GetComponentInChildren<BoxCollider2D>();

        // Add a little to centre the player
        transform.position = new Vector3(position.x, position.y + b.bounds.extents.y, 0);
    }


    public void SetDead()
    {
        isAlive = false;
        controller.enabled = false;
    }


    private void SetAlive()
    {
        // Set player to be alive and enable controls
        isAlive = true;
        controller.enabled = true;

        
    }


    public void PickedUpCoin()
    {
        coinCount++;
    }
}
