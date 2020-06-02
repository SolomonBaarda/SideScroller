using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour, IAttack, ICanBeAttacked
{
    private const float DEFAULT_INTERACT_TIMEOUT_SECONDS = 0.25f;
    [SerializeField]
    private float interact_timeout = DEFAULT_INTERACT_TIMEOUT_SECONDS;

    private List<Collider2D> areaOfInteraction;
    public Collider2D AreaOfAttack { get; private set; }

    private PlayerInventory inventory;
    private Rigidbody2D rigid;

    public void SetColliders(List<Collider2D> areaOfInteraction, Collider2D areaOfAttack)
    {
        this.areaOfInteraction = areaOfInteraction;
        AreaOfAttack = areaOfAttack;

        inventory = GetComponent<PlayerInventory>();
        rigid = GetComponent<Rigidbody2D>();
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
        // First check if the item needs to be dropped
        if (inventory.CanDropPayload())
        {
            // Drop the item
            if (interact1 && interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS)
            {
                interact_timeout = 0;
                inventory.DropPayload();
            }
        }

        List<Collider2D> collisionItems = new List<Collider2D>();

        // Filter colliders to be only item layer
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(ItemManager.ITEM_LAYER));
        filter.useTriggers = true;

        // Loop through each collision with items on each collider
        foreach (Collider2D collider in areaOfInteraction)
        {
            // List will be resized if its too small
            Collider2D[] collisions = new Collider2D[8];

            if (Physics2D.OverlapCollider(collider, filter, collisions) > 0)
            {
                // Ensure the collider has not already been added
                foreach (Collider2D c in collisions)
                {
                    if (c != null)
                    {
                        if (!collisionItems.Contains(c))
                        {
                            collisionItems.Add(c);
                        }
                    }
                }
            }
        }

        // Only bother if there are items
        if (collisionItems.Count > 0)
        {
            // Reverse sort by renderer sorting layer
            collisionItems.Sort(
                (x, y) => -(SortingLayer.GetLayerValueFromID(x.gameObject.GetComponent<SpriteRenderer>().sortingLayerID)
                    .CompareTo(SortingLayer.GetLayerValueFromID(y.gameObject.GetComponent<SpriteRenderer>().sortingLayerID)))
            );

            // Check if any items can be picked up by the player
            foreach (Collider2D c in collisionItems)
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

            foreach (Collider2D c in collisionItems)
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



    public void Attack(bool isAttacking)
    {
        // Vaid time to attack
        if (interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS && isAttacking)
        {
            // Get all the targets
            List<GameObject> targets = InAreaOfAttack();

            if (targets.Count > 0)
            {
                // Reset the timer
                interact_timeout = 0;

                // Loop through each target and hit them
                foreach (GameObject g in targets)
                {
                    // Ensure it can be attacked (sanity check)
                    if (WorldItem.ExtendsClass<ICanBeAttacked>(g))
                    {
                        // Attack that object
                        ICanBeAttacked target = (ICanBeAttacked)WorldItem.GetClass<ICanBeAttacked>(g);
                        target.WasAttacked(transform.position, rigid.velocity);
                    }
                }
            }
        }
    }


    public List<GameObject> InAreaOfAttack()
    {
        // Set the layermask
        // Don't allow player to attack themselves and remove some common layers for performance reasons
        LayerMask layerMask = ~(
            (1 << gameObject.layer) | (1 << LayerMask.NameToLayer(Player.LAYER_ONLY_GROUND)) |
            (1 << LayerMask.NameToLayer(Chunk.CHUNK_LAYER)) | (1 << LayerMask.NameToLayer(TerrainManager.LAYER_NAME_GROUND)) |
            (1 << LayerMask.NameToLayer(Hazard.LAYER))
        );

        // Set the contact filter
        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = layerMask,
            useTriggers = true
        };

        List<GameObject> validTargets = new List<GameObject>();

        // Get all possible collisions
        Collider2D[] allCollisions = new Collider2D[8];
        if (Physics2D.OverlapCollider(AreaOfAttack, contactFilter, allCollisions) > 0)
        {
            foreach (Collider2D c in allCollisions)
            {
                if (c != null)
                {
                    GameObject g = c.gameObject;

                    // Remove all that can't be attacked and ensure there are no duplicates
                    if (WorldItem.ExtendsClass<ICanBeAttacked>(g) && !validTargets.Contains(g))
                    {
                        validTargets.Add(g);
                    }
                }
            }
        }

        return validTargets;
    }



    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        PlayerManager.OnPlayerDie.Invoke(GetComponent<Player>());
    }

}



