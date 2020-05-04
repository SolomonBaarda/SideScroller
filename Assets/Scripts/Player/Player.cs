using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour, ILocatable
{
    private PlayerController controller;
    private PlayerInventory inventory;
    private PlayerInteraction interaction;

    [SerializeField] public bool IsAlive { get; private set; }

    public string PLAYER_ID { get; private set; }
    public string PLAYER_LAYER { get; private set; }
    public const string DEFAULT_PLAYER_LAYER = "Player";


    [SerializeField] public ID PlayerID { get; private set; }

    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public enum ID
    {
        P1,
        P2,
    }


    public void SetPlayer(ID PlayerID)
    {
        this.PlayerID = PlayerID;

        // Set the player
        PLAYER_ID = PlayerID.ToString();
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
        if (IsAlive)
        {
            UpdateCurrentChunk();
        }

    }



    public void SetPosition(Vector2 position)
    {
        BoxCollider2D b = GetComponentInChildren<BoxCollider2D>();
        Rigidbody2D r = GetComponent<Rigidbody2D>();
        Vector2 vel = r.velocity;
        vel.y = 0;
        r.velocity = vel;

        // Add a little to centre the player
        transform.position = new Vector2(position.x, position.y + b.bounds.max.y);
    }


    public void SetDead()
    {
        IsAlive = false;
        controller.enabled = false;

        inventory.DropItem(true);
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


    public void UpdateCurrentChunk()
    {
        CurrentChunk = Chunk.UpdateCurrentChunk(CurrentChunk, Position);
    }



    public static class Default
    {
        public const float SPEED = 40;
        public const float SPEED_MINIMUM = 20;


    }
}
