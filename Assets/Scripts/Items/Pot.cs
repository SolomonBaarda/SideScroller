using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable, ILoot, ICanBeAttacked
{
    public LootTable table;

    [SerializeField]
    private int inventory_size = 1;

    private bool hasContents = true;

    new private void Awake()
    {
        base.Awake();

        SetRendererSortingLayer(ItemManager.RENDERING_LAYER_ITEM_INVENTORY_FRONT);
    }


    public void Interact(PlayerInventory inventory)
    {
        // Break the pot
        Animator a = GetComponent<Animator>();
        a.SetTrigger("Break");
    }


    public LootTable GetLootTable()
    {
        return table;
    }


    public int GetTotalItemsToBeLooted()
    {
        return inventory_size;
    }

    public bool IsLootable()
    {
        return hasContents;
    }

    public void Loot()
    {
        hasContents = false;
    }

    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        InteractionManager.OnPlayerInteractWithItem(gameObject, null);
    }
}
