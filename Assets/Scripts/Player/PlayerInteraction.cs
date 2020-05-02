using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private float interact_timeout = 0;
    [SerializeField] private float DEFAULT_INTERACT_TIMEOUT_SECONDS = 0.25f;

    private List<Collider2D> allColliders;

    private PlayerInventory inventory;

    private void Awake()
    {
        // Add all attached colliders to the list
        allColliders = new List<Collider2D>();
        foreach (Collider2D c in GetComponents<Collider2D>())
        {
            allColliders.Add(c);
        }

        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        // Increase the timer until the player can interact again
        if (interact_timeout < DEFAULT_INTERACT_TIMEOUT_SECONDS)
        {
            interact_timeout += Time.deltaTime;
        }
    }


    public void Interact(bool interact1)
    {
        List<Collider2D> collisionItems = new List<Collider2D>();

        // Filter colliders to be only item layer
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(ItemManager.ITEM_LAYER));
        filter.useTriggers = true;

        // Loop through each collision with items on each collider
        foreach (Collider2D collider in allColliders)
        {
            // List will be resized if its too small
            Collider2D[] collisions = new Collider2D[1];

            Physics2D.OverlapCollider(collider, filter, collisions);

            // Ensure the collider has not already been added
            foreach (Collider2D col in collisions)
            {
                if (col != null)
                {
                    if (!collisionItems.Contains(col))
                    {
                        collisionItems.Add(col);
                    }
                }
            }
        }

        // Only bother if there are items
        if (collisionItems.Count > 0)
        {
            // Sort by renderer sorting layer, always use item in front first
            collisionItems.Sort((x, y) => SortingLayer.GetLayerValueFromID(x.gameObject.GetComponent<SpriteRenderer>().sortingLayerID).CompareTo(SortingLayer.GetLayerValueFromID(y.gameObject.GetComponent<SpriteRenderer>().sortingLayerID)));
            Collider2D[] collisionArray = collisionItems.ToArray();

            // Check if any items can be picked up by the player
            foreach (Collider2D c in collisionArray)
            {
                GameObject g = c.gameObject;

                // If it can be collided with
                if (WorldItem.ExtendsClass<ICollidable>(g))
                {
                    ICollidable collidable = (ICollidable)WorldItem.GetClass<ICollidable>(g);

                    // Collide with it
                    collidable.Collide(inventory);
                }
            }

            foreach (Collider2D c in collisionArray)
            {
                // Need to check if not null as collision may have deleted the item by now
                if (c != null)
                {
                    GameObject g = c.gameObject;

                    // If it can be collided with
                    if (WorldItem.ExtendsClass<IInteractable>(g))
                    {
                        IInteractable interactable = (IInteractable)WorldItem.GetClass<IInteractable>(g);

                        // Check if it is a valid time to interact
                        if (interact1 && interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS)
                        {
                            // Interact with that one item only
                            InteractionManager.OnPlayerInteractWithItem(g, inventory);
                            interact_timeout = 0;
                            break;
                        }
                    }
                }
            }
        }


    }

}



