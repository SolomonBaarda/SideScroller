using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy 
{
    public const string LAYER = "Enemy";

    public abstract void Attack();

    public abstract void Move(Transform destination);



}
