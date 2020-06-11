using System.Collections.Generic;
using UnityEngine;

public interface IWeapon : IInteractable, ICanBeHeld
{
    string Name { get; }

    bool IsAttacking { get; }

    bool WasBlocked { get; }

    /// <summary>
    /// Returns all GameObjects within the weapons current area of attack.
    /// </summary>
    /// <returns></returns>
    List<GameObject> AreaOfAttack { get; }

    /// <summary>
    /// Attack all enemies in area of attack.
    /// </summary>
    /// <param name="attackerPosition"></param>
    /// <param name="attackerVelocity"></param>
    void Attack(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing facing);

    void AttackWasBlocked();
}

