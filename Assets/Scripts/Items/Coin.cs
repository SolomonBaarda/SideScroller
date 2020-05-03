using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : CollectableItem, ILoot
{
    public float initialSetupTime = DEFAULT_INITIAL_SETUP_TIME;

    new protected void Awake()
    {
        base.Awake();
        SetRendererSortingLayer(ItemManager.RENDERING_LAYER_ITEM_COLLISION);
        collideToPickUp = true;
    }


    private void Start()
    {
        DisableFor(initialSetupTime);
    }

}
