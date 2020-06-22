using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy 
{

    void Attack();

    void Move(Transform destination);

}
