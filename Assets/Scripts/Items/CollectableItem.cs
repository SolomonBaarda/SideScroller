using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : WorldItem, ICollidable, IInteractable, ICollectable
{
    [SerializeField]
    protected bool collideToPickUp = false, interactToPickUp = false;
    public const float DEFAULT_INITIAL_SETUP_TIME = 0.5f;

    protected Rigidbody2D rigid;

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

        this.collideToPickUp = collideToPickUp;
        this.interactToPickUp = interactToPickUp;

        DisableFor(initialSetupTime);
    }




    public void Collide(Player player)
    {
        if (collideToPickUp)
        {
            Collect(player);
        }
    }


    public bool Interact(Player player)
    {
        if (interactToPickUp)
        {
            Collect(player);
            return true;
        }
            
        return false;
    }



    public void Collect(Player player)
    {
        // Inventory succesfully picked this up
        if (player.Inventory.PickUp(gameObject))
        {
            trigger.enabled = false;
        }
    }



    public void Drop(Vector2 position, Vector2 velocity)
    {
        // Set position and velocity to throw the item
        transform.parent = null;
        SetPosition(position);

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
