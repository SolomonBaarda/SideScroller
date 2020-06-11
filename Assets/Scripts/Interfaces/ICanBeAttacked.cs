using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeAttacked
{
    /// <summary>
    /// Called when this item is attacked.
    /// </summary>
    /// <param name="attackerPosition"></param>
    /// <param name="attackerVelocity"></param>
    /// <param name="weapon"></param>
    void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity, IWeapon weapon);
}
