using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : WorldItem, ICollidable, IInteractable, ICollectable
{
    [SerializeField]
    private bool collideToPickUp = false, interactToPickUp = false;

    private Rigidbody2D rigid;

    new private void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();

        // Disable the trigger collider by default
        trigger.enabled = false;
    }



    public void SetCollectableItem(ItemBase item, bool collideToPickUp, bool interactToPickUp)
    {
        this.item = item;
        UpdateItemSprite();

        this.collideToPickUp = collideToPickUp;
        this.interactToPickUp = interactToPickUp;

        trigger.enabled = true;
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
        if(inventory.PickUp(gameObject))
        {
            enabled = false;
        }
    }



    public void Drop(Vector2 position, Vector2 velocity)
    {
        // Set position and velocity to throw the item
        transform.position = position;
        rigid.velocity = velocity;

        // Enable it
        enabled = true;
    }


}
