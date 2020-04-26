using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for storing information about the drop chance of specific items, for an item.
/// </summary>
[Serializable]
[CreateAssetMenu]
public class LootTable : ScriptableObject
{
    public Drop[] drops;


    /// <summary>
    /// Returns the Item with value. Value should be between 0 and GetTotalWeight().
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public WorldItem.Name GetLoot(int value)
    {
        int total = 0;
        for (int i = 0; i < drops.Length; i++)
        {
            total += drops[i].dropChance;

            if (value < total)
            {
                return drops[i].item;
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
        foreach (Drop d in drops)
        {
            total += d.dropChance;
        }

        return total;
    }


    [Serializable]
    public class Drop
    {
        [SerializeField]
        public WorldItem.Name item;
        [SerializeField]
        public int dropChance;
    }

}
