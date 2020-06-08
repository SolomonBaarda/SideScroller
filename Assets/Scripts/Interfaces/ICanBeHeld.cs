using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeHeld
{
    Transform GroundPosition { get; }

    void Hold(Player player, Vector2 localPosition);

    void Drop(Vector2 position, Vector2 velocity);

    void SetHeldPosition(Transform t);
}
