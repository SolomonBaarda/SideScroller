using System.Collections.Generic;
using UnityEngine;

public interface IWeapon : IInteractable, ICanBeHeld
{
    string Name { get; }

    bool IsAttacking { get; }

    /// <summary>
    /// The current position state of the weapon.
    /// </summary>
    WeaponPosition Position { get; set; }

    /// <summary>
    /// Returns all GameObjects within the weapons current area of attack.
    /// </summary>
    /// <returns></returns>
    List<GameObject> InAreaOfAttack();

    /// <summary>
    /// Attack all enemies in area of attack.
    /// </summary>
    /// <param name="attackerPosition"></param>
    /// <param name="attackerVelocity"></param>
    void Attack(Vector2 attackerPosition, Vector2 attackerVelocity);
}

public enum WeaponPosition
{
    Up,
    Down,
}
