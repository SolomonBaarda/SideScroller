using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeHeld
{
    Transform GroundPosition { get; }

    void Hold(Player player);

    void Drop(Vector2 position, Vector2 velocity);

    void SetHeldPosition(Transform t);
}
