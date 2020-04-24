﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILootable 
{
    LootTable GetLootTable();

    int GetTotalItemsToBeLooted();

    bool IsLootable();

    void Loot();
}