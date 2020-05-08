using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ILocatable
{
    private PlayerController controller;
    private PlayerInventory inventory;
    private PlayerInteraction interaction;
    private PlayerMovement movement;

    public Transform headPosition, feetPosition;

    public Collider2D torsoCollider, feetCollider, areaOfAttackCollider;

    public bool IsAlive { get; private set; }
    public string PLAYER_ID { get; private set; }
    public string PLAYER_LAYER { get; private set; }
    public const string DEFAULT_PLAYER_LAYER = "Player";

    public const string LAYER_ONLY_GROUND = "OnlyGround";

    [SerializeField] public ID PlayerID { get; private set; }

    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public enum ID
    {
        P1,
        P2,
    }


    public void SetPlayer(ID PlayerID, bool canUseController)
    {
        this.PlayerID = PlayerID;

        // Set the player
        PLAYER_ID = PlayerID.ToString();
        PLAYER_LAYER = DEFAULT_PLAYER_LAYER + "_" + PLAYER_ID;
        gameObject.layer = LayerMask.NameToLayer(PLAYER_LAYER);

        // Controller reference
        controller = GetComponent<PlayerController>();
        controller.enabled = false;
        controller.SetDefaults(headPosition, feetPosition);

        controller.SetControls(PLAYER_ID, canUseController);

        interaction = GetComponent<PlayerInteraction>();
        interaction.SetColliders(new List<Collider2D>(new Collider2D[] { torsoCollider, feetCollider}), areaOfAttackCollider);

        inventory = GetComponent<PlayerInventory>();

        movement = GetComponent<PlayerMovement>();
        movement.SetColliders(torsoCollider, feetCollider);

        // Player variables
        IsAlive = false;
    }



    private void FixedUpdate()
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

        float height = headPosition.position.y - feetPosition.position.y;

        // Add a little to centre the player
        transform.position = new Vector2(position.x, position.y + (height/2));
    }


    public void SetDead()
    {
        IsAlive = false;
        controller.enabled = false;

        inventory.DropItem();
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
