using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableItem : MonoBehaviour
{
    [SerializeField]
    protected LootTable lootTable;

    public bool isInteractable = false;
    public bool canBePickedUp = false;


    public LootTable GetLootTable()
    {
        if (lootTable == null)
        {
            throw new System.Exception("Loot table not defined for " + this);
        }

        return lootTable;
    }


    /// <summary>
    /// Method to be called when the item is interacted with. Return true if an item from the LootTable should be spawned.
    /// </summary>
    /// <returns></returns>
    public abstract bool Interact();


    /// <summary>
    /// Method to be called when the item is collided with. Return true if an item from the LootTable should be spawned.
    /// </summary>
    /// <returns></returns>
    public abstract bool PickUp(PlayerInventory player);


}
