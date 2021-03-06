﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Player : MonoBehaviour, ILocatable
{
    private PlayerController Controller;
    public PlayerInventory Inventory { get; private set; }
    public PlayerInteraction Interaction { get; private set; }
    private PlayerMovement Movement;

    public Facing DirectionFacing { get { return Movement.Facing; } }

    public Transform headPosition, feetPosition, crouchingHeadPosition;
    public Transform Head
    {
        get { if (Movement.IsCrouching) { return crouchingHeadPosition; } else { return headPosition; } }
    }

    public Transform leftHand, leftHandCrouching, rightHand, rightHandUp, rightHandCrouching;


    /// <summary>
    /// The current position of the Player's left hand.
    /// </summary>
    public Transform LeftHand
    {
        get { if (Movement.IsCrouching) { return leftHandCrouching; } else { return leftHand; } }
    }

    /// <summary>
    /// The current position of the Player's right hand.
    /// </summary>
    public Transform RightHand
    {
        get
        {
            if (Movement.IsCrouching) { return rightHandCrouching; }
            else { if (RightHandPosition == HandPosition.Up) { return rightHandUp; } else { return rightHand; } }
        }
    }

    public HandPosition RightHandPosition { get; private set; } = HandPosition.Down;

    public Collider2D torsoCollider, feetCollider;

    public bool IsAlive { get; private set; } = false;
    public int Deaths { get; private set; } = 0;
    public string PLAYER_ID { get; private set; }
    public string LAYER { get; private set; }
    public const string DEFAULT_LAYER = "Player";

    public const string LAYER_ONLY_GROUND = "OnlyGround";

    [SerializeField] public ID PlayerID { get; private set; }

    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }
    public Payload.Direction IdealDirection { get; private set; }
    public Vector2 NearestSpawnPoint { get; private set; }

    private Rigidbody2D rigid;

    public enum ID
    {
        [Description("Player 1")]
        P1,
        [Description("Player 2")]
        P2,
    }


    public void SetPlayer(ID PlayerID, Payload.Direction directionToMove, bool canUseController, float movementSpeed)
    {
        this.PlayerID = PlayerID;
        IdealDirection = directionToMove;

        // Set the player
        PLAYER_ID = PlayerID.ToString();
        LAYER = DEFAULT_LAYER + "_" + PLAYER_ID;
        gameObject.layer = LayerMask.NameToLayer(LAYER);

        // Controller reference
        Controller = GetComponent<PlayerController>();
        Controller.enabled = false;

        Controller.SetControls(PLAYER_ID, canUseController);

        Interaction = GetComponent<PlayerInteraction>();
        Interaction.SetColliders(new List<Collider2D>(new Collider2D[] { torsoCollider, feetCollider }));

        Inventory = GetComponent<PlayerInventory>();

        Movement = GetComponent<PlayerMovement>();
        // Initialise the player movement
        Movement.Initialise(movementSpeed, torsoCollider, headPosition, feetPosition);

        rigid = GetComponent<Rigidbody2D>();

        // Player variables
        IsAlive = false;

        // Call the UpdatePayload method repeatedly
        InvokeRepeating("UpdatePlayer", 1, Chunk.UPDATE_CHUNK_REPEATING_DEFAULT_TIME);
    }



    private void UpdatePlayer()
    {
        if (IsAlive)
        {
            UpdateCurrentChunk();
        }
    }



    public IEnumerator FreezeFor(float seconds)
    {
        DateTime before = DateTime.Now;

        // Freeze player while timer has not finished
        while ((DateTime.Now - before).TotalSeconds < seconds)
        {
            Freeze(true);
            yield return null;
        }

        Freeze(false);
    }



    public void FreezeWhileAtacking()
    {
        // Force the coroutine to run on this script
        StartCoroutine(WaitForFreezeWhileAttacking());
    }


    private IEnumerator WaitForFreezeWhileAttacking()
    {
        IWeapon w = Inventory.GetPrimaryWeapon();

        // Ensure there is a weapon
        while (w != null)
        {
            // Freeze the player while they are attacking
            if (w.IsAttacking)
            {
                Freeze(true);
                yield return null;
            }
            else
            {
                break;
            }
        }

        Freeze(false);
    }


    private void Freeze(bool freeze)
    {
        // Disable the controller script
        Controller.enabled = !freeze;
        Interaction.enabled = !freeze;

        // Disable the movement script if on the ground
        if (freeze && Movement.IsOnGround)
        {
            Movement.ResetVelocity();
        }
    }


    public void SetPosition(Vector2 position)
    {
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        float height = headPosition.position.y - feetPosition.position.y;

        // Add a little to centre the player
        transform.position = new Vector2(position.x, position.y + (height / 2));
    }


    public void SetDead()
    {
        if (IsAlive)
        {
            // Set dead and disable controls
            IsAlive = false;
            Controller.enabled = false;
            Movement.enabled = false;
            gameObject.SetActive(false);

            // Count the deaths
            Deaths++;

            // Drop any items
            Inventory.DropAllHeldItems();
        }
    }


    public void SetAlive()
    {
        // Set player to be alive and enable controls
        IsAlive = true;
        Controller.enabled = true;
        Movement.enabled = true;
        Interaction.enabled = true;
        gameObject.SetActive(true);
    }


    public bool MoveHandPosition(HandPosition direction)
    {
        switch (RightHandPosition)
        {
            case HandPosition.Up:
                switch (direction)
                {
                    // Can't move up as already up
                    case HandPosition.Up:
                        return false;
                    // Move down
                    case HandPosition.Down:
                        RightHandPosition = HandPosition.Down;
                        return true;
                }
                break;
            case HandPosition.Down:
                switch (direction)
                {
                    // Move up
                    case HandPosition.Up:
                        RightHandPosition = HandPosition.Up;
                        return true;
                    // Can't move down as already down
                    case HandPosition.Down:
                        return false;
                }
                break;
        }
        return false;
    }



    public void Face(Facing direction)
    {
        Movement.Face(direction);
    }


    public PlayerInventory.IInventory<T> GetInventory<T>() where T : class
    {
        return Inventory.GetInventory<T>();
    }


    public void UpdateCurrentChunk()
    {
        // Update the chunk
        CurrentChunk = Chunk.UpdateCurrentChunk(CurrentChunk, Position);

        // Get the closest respawn point
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



    public enum HandPosition
    {
        Up,
        Down,
    }


    public enum Facing
    {
        Right,
        Left,
    }

}
