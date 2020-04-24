using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : WorldItem, ICollidable, ILoot
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



    public void Collide(PlayerInventory player)
    {
        trigger.enabled = false;

        player.PickUpCoin();

        Destroy(gameObject);
    }
}
