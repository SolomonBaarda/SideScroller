using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : CollectableItem, ILoot
{
    [SerializeField]
    private float initialSetup = 0.5f;

    new protected void Awake()
    {
        base.Awake();
        trigger.enabled = false;

        SetRendererSortingLayer(ItemManager.RENDERING_LAYER_ITEM_COLLISION);
    }


    private void Start()
    {
        StartCoroutine(InitialDisable());
    }


    private IEnumerator InitialDisable()
    {
        // Disable the being picked up for a little when enabled
        yield return new WaitForSeconds(initialSetup);
        trigger.enabled = true;
    }


}
