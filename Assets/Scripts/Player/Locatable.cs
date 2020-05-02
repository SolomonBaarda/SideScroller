using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Locatable : MonoBehaviour
{
    public Chunk CurrentChunk { get; protected set; }


    protected Chunk CalculateCurrentChunk()
    {
        // Find the chunk at the centre point
        Collider2D collision = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask(Chunk.CHUNK_LAYER));
        if (collision != null)
        {
            return collision.gameObject.GetComponent<Chunk>();
        }

        return null;
    }


    protected void UpdateCurrentChunk()
    {
        // Get the chunk the player is in now
        Chunk current = CalculateCurrentChunk();
        if (current != null)
        {
            // New chunk
            if (current != CurrentChunk)
            {
                CurrentChunk = current;
            }
        }
    }



}
