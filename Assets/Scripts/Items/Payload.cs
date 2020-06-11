using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Payload : CollectableItem, ILocatable, ICanBeAttacked, ICanBeHeld
{
    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public Transform groundPosition;

    public Direction IdealDirection { get; private set; } = Direction.None;

    public Transform GroundPosition { get { return groundPosition; } }

    public bool IsBeingHeld { get; private set; } = false;

    public const float DEFAULT_ATTACKED_COOLDOWN_SECONDS = 0.25f;
    private DateTime lastAttacked;
    public bool CanBeAttacked => !IsBeingHeld && (DateTime.Now - lastAttacked).TotalSeconds >= DEFAULT_ATTACKED_COOLDOWN_SECONDS;


    [Range(0, 100)]
    public int onAttackMultiplier = 8;

    public override void Awake()
    {
        base.Awake();
        interactToPickUp = true;
        trigger.enabled = true;

        lastAttacked = DateTime.Now;

        // Call the UpdatePayload method repeatedly
        InvokeRepeating("UpdatePayload", 1, Chunk.UPDATE_CHUNK_REPEATING_DEFAULT_TIME);
    }


    private void UpdatePayload()
    {
        UpdateCurrentChunk();
        CheckOutOfBounds();
    }


    public void UpdateCurrentChunk()
    {
        // Get the chunk the player is in now
        Chunk currentNew = Chunk.CalculateCurrentChunk(Position);

        // Not null, valid chunk
        if (currentNew != null)
        {
            CurrentChunk = currentNew;
        }
        // Is null, don't update current chunk
        else
        {
            try
            {
                // Try to inform the ItemManager that this is out of bounds
                ItemManager.OnItemOutOfBounds(gameObject);
            }
            catch (System.NullReferenceException)
            {
                // May not have an ItemManager in the scene
            }

        }
    }

    private void CheckOutOfBounds()
    {
        Collider2D[] collisions = new Collider2D[1];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(Hazard.LAYER));
        filter.useTriggers = true;

        // The payload is colliding with a hazard, therefore out of bounds
        if (Physics2D.OverlapCollider(trigger, filter, collisions) > 0)
        {
            // Move to the last valid ground position
            ItemManager.OnItemOutOfBounds(gameObject);
        }
    }




    public void PickUp(GameObject player)
    {
        IsBeingHeld = true;

        // Disable physics while picked up
        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;
        trigger.enabled = false;

        // Update the direction this payload wants to travel in
        Player p = player.GetComponent<Player>();
        IdealDirection = p.IdealDirection;

        // Pick up
        transform.parent = player.transform;
    }

    public new void Drop(Vector2 position, Vector2 velocity)
    {
        IsBeingHeld = false;

        // Enable physics again
        rigid.isKinematic = false;
        trigger.enabled = true;
        transform.parent = null;

        // Set the direction
        IdealDirection = Direction.None;

        // Drop
        base.Drop(position, velocity);
    }

    public void SetHeldPosition(Transform t)
    {
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        transform.rotation = Quaternion.identity;
        transform.localPosition = (Vector2)t.localPosition + -(Vector2)GroundPosition.localPosition;
    }

    public void SetPosition(Vector2 position)
    {
        rigid.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;

        // Add a little to centre the player
        transform.position = position + -(Vector2)GroundPosition.localPosition;
    }


    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity, IWeapon weapon)
    {
        lastAttacked = DateTime.Now;

        // Direction vector from attacker to this
        Vector2 normal = ((Vector2)transform.position - attackerPosition).normalized;

        // Combine with attacker velocity
        rigid.velocity += attackerVelocity;
        // Add a directional force to the payload (boost the y by a little bit)
        rigid.AddForce(normal * new Vector2(1, 1.25f) * onAttackMultiplier, ForceMode2D.Impulse);
    }

    public void Hold(Player player)
    {
        PickUp(player.gameObject);
    }

    public enum Direction
    {
        None,
        Left,
        Right,
    }

}
