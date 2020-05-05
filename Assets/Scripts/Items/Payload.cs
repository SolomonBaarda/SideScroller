using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload : CollectableItem, ILocatable
{
    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }

    public Transform groundPosition;
    private Vector2 lastGroundPosition;


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
        rigid.isKinematic = true;

        transform.parent = player.transform;
        transform.position = newPosition;
    }

    public new void Drop(Vector2 position, Vector2 velocity)
    {
        rigid.isKinematic = false;
        transform.parent = null;

        base.Drop(position, velocity);
    }


}
