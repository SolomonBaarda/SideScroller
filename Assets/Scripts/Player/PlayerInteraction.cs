using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private float interact_timeout = 0;
    [SerializeField] private float DEFAULT_INTERACT_TIMEOUT_SECONDS = 0.25f;

    List<Collider2D> allColliders;

    private void Awake()
    {
        // Add all attached colliders to the list
        allColliders = new List<Collider2D>();
        foreach (Collider2D c in GetComponents<Collider2D>())
        {
            allColliders.Add(c);
        }
    }

    private void Update()
    {
        // Increase the timer until the player can interact again
        if (interact_timeout < DEFAULT_INTERACT_TIMEOUT_SECONDS)
        {
            interact_timeout += Time.deltaTime;
        }
    }


    public void Interact(bool interact1, bool interact2)
    {
        List<Collider2D> collisionItems = new List<Collider2D>();

        // Loop through each collision with items on each collider
        foreach (Collider2D collider in allColliders)
        {
            // List will be resized if its too small
            Collider2D[] collisions = new Collider2D[1];

            // Filter colliders to be only item layer
            ContactFilter2D c = new ContactFilter2D();
            c.SetLayerMask(LayerMask.GetMask(ItemManager.ITEM_LAYER));
            c.useTriggers = true;

            Physics2D.OverlapCollider(collider, c, collisions);

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

        // Remove all elements that arent interactable items
        //collisionItems.RemoveAll(c => c.gameObject.GetComponent<InteractableItem>() == null);

        // Only bother if there are items
        if (collisionItems.Count > 0)
        {
            // Sort by distance to player
            collisionItems.Sort((x, y) => Vector2.Distance(transform.position, x.transform.position).CompareTo(Vector2.Distance(transform.position, y.transform.position)));
            Collider2D[] collisionArray = collisionItems.ToArray();

            // Check if any items can be picked up by the player
            foreach (Collider2D c in collisionArray)
            {
                GameObject g = c.gameObject;

                // If it can be collided with
                if (WorldItem.ImplementsInterface<ICollidable>(g))
                {
                    ICollidable collidable = (ICollidable)WorldItem.GetScriptThatImplements<ICollidable>(g);

                    // Collide with it
                    collidable.Collide(GetComponent<PlayerInventory>());
                }
            }

            foreach (Collider2D c in collisionArray)
            {
                // Need to check if not null as collision may have deleted the item by now
                if (c != null)
                {
                    GameObject g = c.gameObject;

                    // If it can be collided with
                    if (WorldItem.ImplementsInterface<IInteractable>(g))
                    {
                        IInteractable interactable = (IInteractable)WorldItem.GetScriptThatImplements<IInteractable>(g);

                        // Check if it is a valid time to interact
                        if (interact1 && interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS)
                        {
                            // Interact with that one item only
                            InteractionManager.OnPlayerInteractWithItem(g);
                            interact_timeout = 0;
                            break;
                        }
                    }
                }

            }
        }


    }

}



