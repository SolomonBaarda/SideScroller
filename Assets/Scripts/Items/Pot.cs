using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable
{
    private enum PotState { Whole, Broken };
    [SerializeField] private PotState state;

    [SerializeField]
    private LootTable table;

    [SerializeField]
    private int inventory_size = 1;

    new private void Awake()
    {
        base.Awake();

        state = PotState.Whole;

        SetRendererSortingLayer(ItemManager.RENDERING_LAYER_ITEM_INVENTORY_FRONT);
    }


    public void Interact(Player player)
    {
        state = PotState.Broken;
        Animator a = GetComponent<Animator>();
        a.SetBool("isBroken", state.Equals(PotState.Broken));
    }


    public LootTable GetLootTable()
    {
        return table;
    }


    public int GetTotalItemsToBeLooted()
    {
        return inventory_size;
    }
}
