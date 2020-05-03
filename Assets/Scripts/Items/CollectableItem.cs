﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : WorldItem, ICollidable, IInteractable, ICollectable
{
    [SerializeField]
    private bool collideToPickUp = false, interactToPickUp = false;
    public const float DEFAULT_INITIAL_SETUP_TIME = 0.5f;

    private Rigidbody2D rigid;

    public override void Awake()
    {
        base.Awake();

        // Disable the trigger collider by default
        trigger.enabled = false;

        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody2D>();
        }
    }


    public void SetCollectableItem(ItemBase item, bool collideToPickUp, bool interactToPickUp, float initialSetupTime = DEFAULT_INITIAL_SETUP_TIME)
    {
        this.item = item;
        base.Awake();

        SetRendererSortingLayer(ItemManager.RENDERING_LAYER_ITEM_COLLISION);

        this.collideToPickUp = collideToPickUp;
        this.interactToPickUp = interactToPickUp;

        DisableFor(initialSetupTime);
    }




    public void Collide(PlayerInventory inventory)
    {
        if (collideToPickUp)
        {
            Collect(inventory);
        }
    }


    public void Interact(PlayerInventory inventory)
    {
        if (interactToPickUp)
        {
            Collect(inventory);
        }
    }



    public void Collect(PlayerInventory inventory)
    {
        // Inventory succesfully picked this up
        if (inventory.PickUp(gameObject))
        {
            trigger.enabled = false;
        }
    }



    public void Drop(Vector2 position, Vector2 velocity)
    {
        // Set position and velocity to throw the item
        transform.position = position;
        rigid.velocity = velocity;

        // Enable it
        enabled = true;
        trigger.enabled = true;
    }



    protected void DisableFor(float seconds)
    {
        // Disable trigger 
        trigger.enabled = false;
        StartCoroutine(EnableTriggerAfter(seconds));
    }


    private IEnumerator EnableTriggerAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        trigger.enabled = true;
    }

}
