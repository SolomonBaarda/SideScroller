using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : WorldItem, ICollidable, IInteractable
{
    [SerializeField]
    private bool collideToPickUp = false, interactToPickUp = false;

    private Rigidbody2D rigid;

    new private void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();

        // Disable the trigger collider by default
        //trigger.enabled = false;
    }



    public void SetCollectableItem(Item item, bool collideToPickUp, bool interactToPickUp)
    {
        this.item = item;
        UpdateItemSprite();

        this.collideToPickUp = collideToPickUp;
        this.interactToPickUp = interactToPickUp;
    }



    public void Collide(PlayerInventory player)
    {
        if (collideToPickUp)
        {
            Debug.Log("player collided");
        }
    }


    public void Interact()
    {
        if (interactToPickUp)
        {
            Debug.Log("player interacted");
        }
    }
}
