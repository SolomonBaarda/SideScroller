using System;
using UnityEngine;

/// <summary>
/// A class for storing information about the drop chance of specific items, for an item.
/// </summary>
[Serializable]
[CreateAssetMenu]
public class LootTable : ScriptableObject
{
    [Range(0, 16)]
    public int InventorySize = 1;

    public Drop[] Drops;

    /// <summary>
    /// Returns the Item with value. Value should be between 0 and GetTotalWeight().
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public GameObject GetLoot(int value)
    {
        int total = 0;
        for (int i = 0; i < Drops.Length; i++)
        {
            total += Drops[i].dropChance;

            if (value < total)
            {
                return Drops[i].itemPrefab;
            }
        }

        throw new Exception("Could not calculate loot drop from table " + this);
    }


    /// <summary>
    /// The total weight of all items in the table. 
    /// </summary>
    /// <returns></returns>
    public int GetTotalWeight()
    {
        int total = 0;
        foreach (Drop d in Drops)
        {
            total += d.dropChance;
        }

        return total;
    }


    [Serializable]
    public class Drop
    {
        [SerializeField]
        public GameObject itemPrefab;
        [SerializeField]
        public int dropChance;
    }

}
