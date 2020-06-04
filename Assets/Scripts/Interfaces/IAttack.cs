using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack 
{
    IWeapon Weapon { get; }

    void Attack(bool attack);
}
