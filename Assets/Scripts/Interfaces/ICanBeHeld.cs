using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeHeld
{
    bool IsBeingHeld { get; }

    Transform GroundPosition { get; }

    void Hold(Player player);

    void Drop(Vector2 position, Vector2 velocity);

    void SetHeldPosition(Transform t);
}
