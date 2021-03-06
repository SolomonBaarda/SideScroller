﻿using UnityEngine;

public class Chest : WorldItem, IInteractable, ILootable
{
    public LootTable lootTable;
    public LootTable Table => lootTable;
    public int TotalItemsToBeLooted => Table.InventorySize;

    public bool IsLootable => contents == ChestContents.Full && state == ChestState.Open;

    private enum ChestState { Locked, Closed, Open };
    [SerializeField] private ChestState state = ChestState.Closed;

    private enum ChestContents { Full, Empty };
    [SerializeField] private ChestContents contents = ChestContents.Full;

    public Transform groundPosition;

    private Animator a;

    new private void Awake()
    {
        base.Awake();

        a = GetComponent<Animator>();
    }


    public new void SetPosition(Vector2 position)
    {
        transform.position = position + -(Vector2)groundPosition.localPosition;
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
            }
        }
        // CLose the chest
        else if (state == ChestState.Open)
        {
            if (state.Equals(ChestState.Open))
            {
                state = ChestState.Closed;
            }
        }

        // Update the animations
        a.SetBool("IsOpen", state == ChestState.Open);

        return true;
    }


    public void Loot()
    {
        contents = ChestContents.Empty;
    }

}
