﻿using UnityEngine;

public class Chest : WorldItem, IInteractable, ILootable
{
    private enum ChestState { Locked, Closed, Open };
    [SerializeField] private ChestState state = ChestState.Closed;

    private enum ChestContents { Full, Empty };
    [SerializeField] private ChestContents contents = ChestContents.Full;

    public LootTable table;

    private Animator a;

    new private void Awake()
    {
        base.Awake();

        a = GetComponent<Animator>();
    }


    public bool Interact(Player player)
    {
        // Unlock the chest
        if (state == ChestState.Locked)
        {
            if (state.Equals(ChestState.Locked))
            {
                state = ChestState.Closed;
            }
        }
        // Open the chest
        else if (state == ChestState.Closed)
        {
            if (state.Equals(ChestState.Closed))
            {
                state = ChestState.Open;
                a.SetTrigger("Open");
            }
        }
        // CLose the chest
        else if (state == ChestState.Open)
        {
            if (state.Equals(ChestState.Open))
            {
                a.SetTrigger("Close");
                state = ChestState.Closed;
            }
        }

        return true;
    }

    public LootTable GetLootTable()
    {
        return table;
    }

    public int GetTotalItemsToBeLooted()
    {
        return table.InventorySize;
    }


    public bool IsLootable()
    {
        return contents == ChestContents.Full && state == ChestState.Open;
    }

    public void Loot()
    {
        contents = ChestContents.Empty;
    }

}
