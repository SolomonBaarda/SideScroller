using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableItem : MonoBehaviour
{
    [SerializeField]
    public LootTable lootTable;

    private void Awake()
    {
        if (lootTable == null)
        {
            throw new System.Exception("Loot table not defined for " + this);
        }
    }


    public LootTable GetLootTable()
    {
        return lootTable;
    }


    /// <summary>
    /// Method to be called when the item is interacted with. Return true if an item from the LootTable should be spawned.
    /// </summary>
    /// <returns></returns>
    public abstract bool Interact();


}
