using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload : CollectableItem, ILocatable
{
    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public Transform groundPosition;
    private Vector2 lastGroundPosition;

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
    }

    public void UpdateCurrentChunk()
    {
        // Get the chunk the player is in now
        Chunk currentNew = Chunk.CalculateCurrentChunk(Position);

        // Not null, valid chunk
        if (currentNew != null)
        {
            CurrentChunk = currentNew;

            if (GroundCheck.IsOnGround(groundPosition.position))
            {
                lastGroundPosition = transform.position;
            }
        }
        // Is null, don't update current chunk
        else
        {
            // Move to the last valid ground position
            //Drop(lastGroundPosition, Vector2.zero);
        }
    }



    public void PickUp(GameObject player, Vector2 newPosition)
    {
        // Disable physics while picked up
        rigid.isKinematic = true;
        trigger.enabled = false;

        // Update the direction this payload wants to travel in
        Player p = player.GetComponent<Player>();
        IdealDirection = p.IdealDirection;

        // Pick up
        transform.parent = player.transform;
        transform.position = newPosition;
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




    public enum Direction
    {
        None,
        Left,
        Right,
    }

}
