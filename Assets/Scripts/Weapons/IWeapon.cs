using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    string Name { get; }

    bool IsAttacking { get; }

    List<GameObject> InAreaOfAttack();

    void Attack(Vector2 attackerPosition, Vector2 attackerVelocity);
}
