using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack 
{
    Collider2D AreaOfAttack { get; }

    void Attack(bool attack);

    List<GameObject> InAreaOfAttack();
}
