using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack 
{
    void Attack(bool attack);

    List<GameObject> InAreaOfAttack();
}
