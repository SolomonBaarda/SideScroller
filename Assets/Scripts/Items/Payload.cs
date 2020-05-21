using System.Collections.Generic;
using UnityEngine;

public class Payload : CollectableItem, ILocatable
{

    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public Transform groundPosition;

    public Direction IdealDirection { get; private set; } = Direction.None;

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
            // Move to the last valid ground position
            ItemManager.OnItemOutOfBounds(gameObject);
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

        localPosition.y += GetHeightExtent();

        // Pick up
        transform.parent = player.transform;
        transform.localPosition = localPosition;
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


    public enum Direction
    {
        None,
        Left,
        Right,
    }

}
