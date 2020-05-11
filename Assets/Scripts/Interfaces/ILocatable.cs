using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocatable
{
    /// <summary>
    /// Reference to the Chunk that the object is currently in.
    /// </summary>
    Chunk CurrentChunk { get; }

    /// <summary>
    /// The ideal direction this object wants to travel in.
    /// </summary>
    Payload.Direction IdealDirection { get; }

    /// <summary>
    /// The current world position of the object.
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// Function that updates the CurrentChunk reference. Should call static methods in Chunk.
    /// </summary>
    void UpdateCurrentChunk();



}
