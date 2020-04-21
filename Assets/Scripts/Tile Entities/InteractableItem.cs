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


    public abstract void Interact();


}
