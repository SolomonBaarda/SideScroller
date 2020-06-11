using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable, IAmLoot, ICanBeAttacked, ICanBeHeld, IWeapon
{
    public LootTable lootTable;
    public LootTable Table => lootTable;

    [SerializeField]
    private bool hasContents = true;

    private Rigidbody2D rigid;

    public Transform groundPosition;
    public Transform GroundPosition { get { return groundPosition; } }

    public bool IsBeingHeld { get; private set; } = false;
    public int TotalItemsToBeLooted => Table.InventorySize;
    public bool IsLootable => hasContents && !IsBeingHeld; 

    public bool IsAttacking { get; private set; } = false;
    private bool isBroken = false;

    public const float ThrownForce = 14f;

    public string Name => "Pot";

    public bool WasBlocked { get; private set; } = false;

    // Return an empty list
    List<GameObject> IWeapon.AreaOfAttack => new List<GameObject>();

    public bool CanBeAttacked => !isBroken;

    new private void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
    }


    public bool Interact(Player player)
    {
        return player.Inventory.PickUp(gameObject);
    }



    public void Loot()
    {
        hasContents = false;
    }

    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity, IWeapon weapon)
    {
        Break();
    }

    private void Break()
    {
        if(!isBroken)
        {
            isBroken = true;

            StopAllCoroutines();
            InteractionManager.OnInteractWithItem(gameObject);

            // Break the pot
            Animator a = GetComponent<Animator>();
            a.SetTrigger("Break");
        }
    }

    public void Hold(Player player)
    {
        IsBeingHeld = true;

        transform.parent = player.transform;

        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;

        trigger.enabled = false;

        // Disable all coroutines if this is picked up
        // e.g. player caught thrown pot out of the air
        StopAllCoroutines();
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        IsBeingHeld = false;

        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;

        trigger.enabled = true;

        // Break the pot when it hits the ground
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(Hazard.LAYER, GroundCheck.GROUND_LAYER));
        filter.useTriggers = true;

        StartCoroutine(WaitForThenInvoke(gameObject, filter, Break));
    }

    public void SetHeldPosition(Transform t)
    {
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        transform.rotation = Quaternion.identity;
        transform.localPosition = (Vector2)t.localPosition + -(Vector2)GroundPosition.localPosition;
    }


    public void Attack(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing facing, ref GameObject inventoryReferenceToThisWeapon)
    {
        if(!IsAttacking)
        {
            // Attack
            IsAttacking = true;
            // Unassign the weapon from the inventory
            inventoryReferenceToThisWeapon = null;

            // Throw the pot in the correct direction
            int xAxis = facing == Player.Facing.Right ? 1 : -1;
            Vector2 direction = new Vector2(xAxis * 1.15f, 1);

            Drop(attackerPosition, attackerVelocity + direction * ThrownForce);
        }
    }

    public void AttackWasBlocked()
    {
        rigid.velocity = Vector2.zero;
    }
}
