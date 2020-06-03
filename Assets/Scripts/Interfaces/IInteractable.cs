using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// Method to be called when the item is interacted with. 
    /// </summary>
    /// <returns></returns>
    void Interact(Player player);

}
