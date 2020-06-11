using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlock : ICanBeAttacked
{
    /// <summary>
    /// Returns true if this item succesfully blocked an attack from the weapon.
    /// </summary>
    /// <param name="weapon"></param>
    /// <returns></returns>
    bool DidBlock(IWeapon weapon);

}
