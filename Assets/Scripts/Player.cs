using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isAlive;

    public int coinCount;

    public PlayerMovement controller;

    public Vector2Int currentChunk;

    public static string PLAYER = "Player";

    private void Awake()
    {
        // Get reference to the controller script
        controller = GetComponent<PlayerMovement>();
        //controller.enabled = false;

        isAlive = false;
        coinCount = 0;
        currentChunk = Vector2Int.one;

        GameManager.OnGameStart += SetAlive;
        ChunkManager.OnPlayerEnterChunk += SetCurrentChunk;
    }

    private void Update()
    {
        if (Input.GetKey(controller.keys.slow))
        {
            SetAlive();
        }
    }


    private void SetCurrentChunk(Vector2Int currentChunk)
    {
        this.currentChunk = currentChunk;
    }

    public Vector2Int GetCurrentChunk()
    {
        return currentChunk;
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
