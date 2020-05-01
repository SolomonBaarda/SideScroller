using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerController controller;
    private PlayerInventory inventory;

    [SerializeField]
    public bool IsAlive { get; private set; }
    public Chunk currentChunk;

    public string PLAYER_ID { get; private set; }
    public string PLAYER_LAYER { get; private set; }
    public const string DEFAULT_PLAYER_LAYER = "Player";


    [SerializeField]
    private ID id;

    private enum ID
    {
        P1,
        P2,
    }

    private void Awake()
    {
        // Set the player
        PLAYER_ID = id.ToString();
        PLAYER_LAYER = DEFAULT_PLAYER_LAYER + "_" + PLAYER_ID;
        gameObject.layer = LayerMask.NameToLayer(PLAYER_LAYER);

        // Controller reference
        controller = GetComponent<PlayerController>();
        controller.enabled = false;
        controller.SetPlayer(PLAYER_ID);

        inventory = GetComponent<PlayerInventory>();

        // Player variables
        IsAlive = false;
    }



    private void Update()
    {
        if(IsAlive)
        {
            // Get the chunk the player is in now
            Chunk current = CalculateCurrentChunk();
            if (current != null)
            {
                // New chunk
                if (current != currentChunk)
                {
                    currentChunk = current;
                }
            }
        }

    }



    private Chunk CalculateCurrentChunk()
    {
        // Find the chunk at the centre point
        Collider2D collision = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask(Chunk.CHUNK_LAYER));
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
        IsAlive = false;
        controller.enabled = false;
    }


    public void SetAlive()
    {
        // Set player to be alive and enable controls
        IsAlive = true;
        controller.enabled = true;
    }



    public PlayerInventory.Inventory<T> GetInventory<T>() where T : class
    {
        return inventory.GetInventory<T>();
    }
}
