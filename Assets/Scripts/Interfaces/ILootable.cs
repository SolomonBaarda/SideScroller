using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILootable 
{
    LootTable Table { get; }

    int TotalItemsToBeLooted { get; }

    bool IsLootable { get; }

    void Loot();
}
