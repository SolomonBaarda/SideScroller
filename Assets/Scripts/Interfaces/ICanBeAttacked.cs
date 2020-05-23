using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeAttacked
{
    void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity);

}
