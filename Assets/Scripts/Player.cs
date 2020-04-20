﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isAlive;

    public int coinCount;

    public PlayerController controller;

    public Chunk currentChunk;

    public static string PLAYER = "Player";

    private void Awake()
    {
        // Get reference to the controller script
        controller = GetComponent<PlayerController>();

        isAlive = false;
        coinCount = 0;

        GameManager.OnGameStart += SetAlive;
    }


    private void Start()
    {
        controller.enabled = false;
    }


    private void FixedUpdate()
    {
        // Get the chunk
        Chunk current = CalculateCurrentChunk();
        if (current != null)
        {
            // New chunk
            if (current != currentChunk)
            {
                currentChunk = current;
            }
        }


        if (Input.GetKey(controller.keys.slow))
        {
            SetAlive();
        }
    }



    private Chunk CalculateCurrentChunk()
    {
        // Find the chunk at the centre point
        Collider2D collision = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask(Chunk.CHUNK));
        if (collision != null)
        {
            return collision.gameObject.GetComponent<Chunk>();
        }

        return null;
    }

    public Chunk GetCurrentChunk()
    {
        return currentChunk;
    }

    public void SetPosition(Vector2 position)
    {
        BoxCollider2D b = GetComponentInChildren<BoxCollider2D>();
        Rigidbody2D r = GetComponent<Rigidbody2D>();
        Vector2 vel = r.velocity;
        vel.y = 0;
        r.velocity = vel;

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
