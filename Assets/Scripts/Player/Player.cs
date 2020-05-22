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

    public bool IsAlive { get; private set; } = false;
    public int Deaths { get; private set; } = 0;
    public string PLAYER_ID { get; private set; }
    public string PLAYER_LAYER { get; private set; }
    public const string DEFAULT_PLAYER_LAYER = "Player";

    public const string LAYER_ONLY_GROUND = "OnlyGround";

    [SerializeField] public ID PlayerID { get; private set; }

    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }
    public Payload.Direction IdealDirection { get; private set; }
    public Vector2 NearestSpawnPoint { get; private set; }

    private Rigidbody2D rigid;

    public enum ID
    {
        P1,
        P2,
    }


    public void SetPlayer(ID PlayerID, Payload.Direction directionToMove, bool canUseController)
    {
        this.PlayerID = PlayerID;
        IdealDirection = directionToMove;

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

        renderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();

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
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        float height = headPosition.position.y - feetPosition.position.y;

        // Add a little to centre the player
        transform.position = new Vector2(position.x, position.y + (height/2));
    }


    public void SetDead()
    {
        if(IsAlive)
        {
            // Set dead and disable controls
            IsAlive = false;
            controller.enabled = false;
            gameObject.SetActive(false);

            // Count the deaths
            Deaths++;

            // Drop any items
            inventory.DropItem();
        }
    }


    public void SetAlive()
    {
        // Set player to be alive and enable controls
        IsAlive = true;
        controller.enabled = true;
        gameObject.SetActive(true);
    }





    public PlayerInventory.Inventory<T> GetInventory<T>() where T : class
    {
        return inventory.GetInventory<T>();
    }


    public void UpdateCurrentChunk()
    {
        CurrentChunk = Chunk.UpdateCurrentChunk(CurrentChunk, Position);

        Vector2 closest = CurrentChunk.respawnPoints[0].position;
        foreach (TerrainManager.TerrainChunk.Respawn r in CurrentChunk.respawnPoints)
        {
            if (Vector2.Distance(transform.position, r.position) < Vector2.Distance(transform.position, closest))
            {
                closest = r.position;
            }
        }
        NearestSpawnPoint = closest;
    }



    public static class Default
    {
        public const float SPEED = 40;
        public const float SPEED_MINIMUM = 20;
    }

}
