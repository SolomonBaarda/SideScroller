using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload : CollectableItem, ILocatable
{
    public Chunk CurrentChunk { get; private set; }
    public Vector2 Position { get { return transform.position; } }


    public override void Awake()
    {
        base.Awake();
    }

    public void UpdateCurrentChunk()
    {
        CurrentChunk = Chunk.UpdateCurrentChunk(CurrentChunk, Position);
    }

}
