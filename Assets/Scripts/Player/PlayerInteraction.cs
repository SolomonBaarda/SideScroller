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

    public IWeapon Weapon => inventory.GetPrimaryWeapon();

    private PlayerInventory inventory;
    private Player player;
    private Rigidbody2D rigid;

    public void SetColliders(List<Collider2D> areaOfInteraction, Collider2D areaOfAttack)
    {
        this.areaOfInteraction = areaOfInteraction;
        AreaOfAttack = areaOfAttack;

        inventory = GetComponent<PlayerInventory>();
        player = GetComponent<Player>();
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
        List<Collider2D> collisionItems = new List<Collider2D>();

        // Filter colliders to be only item layer
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(ItemManager.ITEM_LAYER));
        filter.useTriggers = true;

        // Loop through each collision with items on each collider
        foreach (Collider2D collider in areaOfInteraction)
        {
            // List will be resized if its too small
            Collider2D[] collisions = new Collider2D[WorldItem.DEFAULT_MAX_OVERLAP_COLLISIONS];

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
                (x, y) => -x.gameObject.GetComponent<SpriteRenderer>().sortingOrder.CompareTo(y.gameObject.GetComponent<SpriteRenderer>().sortingOrder)
            );

            // Collide with all items
            foreach (Collider2D c in collisionItems)
            {
                GameObject g = c.gameObject;

                // If it can be collided with
                if (WorldItem.ExtendsClass<ICollidable>(g))
                {
                    ICollidable collidable = (ICollidable)WorldItem.GetClass<ICollidable>(g);

                    // Collide with it
                    collidable.Collide(player);
                }
            }

            // Interact with all items
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
                            interactable.Interact(player);
                            InteractionManager.OnInteractWithItem(g);
                            interact_timeout = 0;
                            return;
                        }
                    }
                }
            }

            // Finally, check if any items need to be dropped
            // If we get here then the player can't do anything else
            if (inventory.IsHoldingItems())
            {
                // Drop the item
                if (interact1 && interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS)
                {
                    if (inventory.DropLeftHand() || inventory.DropRightHand())
                    {
                        interact_timeout = 0;
                        return;
                    }
                }
            }
        }
    }


    public void Attack(bool attack)
    {
        // Ensure it is a valid time to attack
        if (interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS && attack)
        {
            // Attack with the weapon
            Weapon.Attack(transform.position, rigid.velocity);

            // Reset the timer
            interact_timeout = 0;
        }
    }




    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        PlayerManager.OnPlayerDie.Invoke(player);
    }







    /// <summary>
    /// Checks the Collider2D area for any GameObjects implementing ICanBeAttacked. Filters out thisObject.layer.
    /// </summary>
    /// <param name="area"></param>
    /// <param name="thisObject"></param>
    /// <returns></returns>
    public static List<GameObject> InAreaOfAttack(Collider2D area, GameObject thisObject)
    {
        // Set the layermask
        // Don't allow player to attack themselves and remove some common layers for performance reasons
        LayerMask layerMask = ~(
            (1 << thisObject.layer) | (1 << LayerMask.NameToLayer(Player.LAYER_ONLY_GROUND)) |
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
        Collider2D[] allCollisions = new Collider2D[WorldItem.DEFAULT_MAX_OVERLAP_COLLISIONS];
        if (Physics2D.OverlapCollider(area, contactFilter, allCollisions) > 0)
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


}



