using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// Method to be called when the item is interacted with. Should make a call to the ItemManager to spawn an item if needed.
    /// </summary>
    /// <returns></returns>
    void Interact(Player player);

}
