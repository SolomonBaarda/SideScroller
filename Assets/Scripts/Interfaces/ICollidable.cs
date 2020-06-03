using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollidable
{
    /// <summary>
    /// Method to be called when the item is collided with. 
    /// </summary>
    /// <returns></returns>
    void Collide(Player player);
}
