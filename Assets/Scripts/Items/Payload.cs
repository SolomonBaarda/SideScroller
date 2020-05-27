using System.Collections.Generic;
using UnityEngine;

public class Payload : CollectableItem, ILocatable, ICanBeAttacked
{
    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public Transform groundPosition;

    public Direction IdealDirection { get; private set; } = Direction.None;

    [Range(0, 100)]
    public int onAttackMultiplier = 10;

    public override void Awake()
    {
        base.Awake();
        SetRendererSortingLayer("Front");
        interactToPickUp = true;
        trigger.enabled = true;
    }


    private void FixedUpdate()
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




    public void PickUp(GameObject player, Vector2 localPosition)
    {
        // Disable physics while picked up
        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;
        trigger.enabled = false;

        // Update the direction this payload wants to travel in
        Player p = player.GetComponent<Player>();
        IdealDirection = p.IdealDirection;

        // Pick up
        transform.parent = player.transform;
        SetLocalPosition(localPosition);
    }

    public new void Drop(Vector2 position, Vector2 velocity)
    {
        // Enable physics again
        rigid.isKinematic = false;
        trigger.enabled = true;
        transform.parent = null;

        // Set the direction
        IdealDirection = Direction.None;

        // Drop
        base.Drop(position, velocity);
    }

    public void SetLocalPosition(Vector2 local)
    {
        // Add a little to centre the height
        local.y += GetHeightExtent();
        transform.localPosition = local;
    }

    public void SetPosition(Vector2 position)
    {
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        // Add a little to centre the player
        transform.position = new Vector2(position.x, position.y + GetHeightExtent());
    }

    private float GetHeightExtent()
    {
        return Mathf.Abs(transform.position.y - groundPosition.position.y);
    }

    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        // Direction vector from attacker to this
        Vector2 normal = ((Vector2)transform.position - attackerPosition).normalized;

        // Combine with attacker velocity
        rigid.velocity += attackerVelocity;
        // Add a directional force to the payload
        rigid.AddForce(normal * onAttackMultiplier, ForceMode2D.Impulse);
    }

    public enum Direction
    {
        None,
        Left,
        Right,
    }

}
