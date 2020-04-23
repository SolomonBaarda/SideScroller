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
        boxCollider.enabled = false;

        SetRendererSortingLayer(ItemManager.RENDERING_LAYER_ITEM_COLLISION);
    }

    private void Start()
    {
        StartCoroutine(InitialDisable());
    }

    private IEnumerator InitialDisable()
    {
        yield return new WaitForSeconds(initialSetup);
        boxCollider.enabled = true;
    }



    public void Collide(PlayerInventory player)
    {
        boxCollider.enabled = false;

        player.PickUpCoin();

        Destroy(gameObject);
    }
}
