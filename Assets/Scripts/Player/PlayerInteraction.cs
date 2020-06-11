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

    public bool CanBeAttacked => player.IsAlive;

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



    public bool UpdateHandAndWeaponPosition(float direction)
    {
        // Ensure valid direction
        if (direction != 0)
        {
            Player.HandPosition weaponDirecion = Player.HandPosition.Up;

            // Move up
            if (direction < 0)
            {
                weaponDirecion = Player.HandPosition.Up;
            }
            // Move down
            else if (direction > 0)
            {
                weaponDirecion = Player.HandPosition.Down;
            }

            // Move the weapon
            bool didMove = player.MoveHandPosition(weaponDirecion);
            if (Weapon != null)
            {
                //Weapon.Position = player.RightHandPosition;
            }
            return didMove;
        }


        return false;
    }



    /// <summary>
    /// Returns the Collider2D of all Items within the AreaOfInteraction.
    /// </summary>
    /// <returns></returns>
    private List<Collider2D> GetValidInteractionCollisions()
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

        return collisionItems;
    }


    public void Interact(bool interact1)
    {
        List<Collider2D> collisionItems = GetValidInteractionCollisions();

        // Only bother if there are items
        if (collisionItems.Count > 0)
        {
            // Reverse sort by renderer sorting layer - use front most Item first
            collisionItems.Sort(
                (x, y) => -x.gameObject.GetComponent<SpriteRenderer>().sortingOrder.CompareTo(y.gameObject.GetComponent<SpriteRenderer>().sortingOrder)
            );

            // Do basic collision with Item
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


            // Ensure it is a valid time to interact
            if (interact1 && interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS)
            {
                // Do basic interact with Item
                foreach (Collider2D c in collisionItems)
                {
                    // Need to check if not null as ICollidable may have deleted the item by now
                    if (c != null)
                    {
                        GameObject g = c.gameObject;

                        // If it can be interacted with
                        if (WorldItem.ExtendsClass<IInteractable>(g))
                        {
                            IInteractable interactable = (IInteractable)WorldItem.GetClass<IInteractable>(g);

                            // Ensure it is a valid interact
                            if (interactable.Interact(player))
                            {
                                // Interact with that one item only
                                InteractionManager.OnInteractWithItem(g);
                                interact_timeout = 0;
                                return;
                            }
                        }
                    }
                }
            }
        }

        // Check if any items need to be dropped
        // If we get here then there aren't any Items for the player to interact with
        if (inventory.IsHoldingItems())
        {
            if (interact1 && interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS)
            {
                // Drop the item - always try left hand first
                if (inventory.DropLeftHand() || inventory.DropRightHand())
                {
                    interact_timeout = 0;
                }
            }
        }
    }


    public void Attack(bool attack)
    {
        // Ensure it is a valid time to attack
        if (interact_timeout >= DEFAULT_INTERACT_TIMEOUT_SECONDS && attack)
        {
            if (Weapon != null)
            {
                // Attack with the weapon
                Weapon.Attack(player.RightHand.transform.position, rigid.velocity, player.DirectionFacing, ref inventory.HeldItemRight);

                // Reset the timer
                interact_timeout = 0;
            }
        }
    }




    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity, IWeapon weapon)
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



