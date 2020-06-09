using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable, IAmLoot, ICanBeAttacked, ICanBeHeld, IWeapon
{
    public LootTable table;

    [SerializeField]
    private bool hasContents = true;

    private Rigidbody2D rigid;

    public Transform groundPosition;

    public Transform GroundPosition { get { return groundPosition; } }

    public bool IsBeingHeld { get; private set; } = false;


    public bool IsAttacking { get; private set; } = false;

    public string Name => "Pot";


    new private void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
    }


    public bool Interact(Player player)
    {
        return player.Inventory.PickUp(gameObject);
    }


    public LootTable GetLootTable()
    {
        return table;
    }


    public int GetTotalItemsToBeLooted()
    {
        return table.InventorySize;
    }

    public bool IsLootable()
    {
        return hasContents && !IsBeingHeld;
    }

    public void Loot()
    {
        hasContents = false;
    }

    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        Break();
    }

    private void Break()
    {
        StopAllCoroutines();
        InteractionManager.OnInteractWithItem(gameObject);

        // Break the pot
        Animator a = GetComponent<Animator>();
        a.SetTrigger("Break");
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


    public List<GameObject> InAreaOfAttack()
    {
        throw new System.NotImplementedException();
    }

    public void Attack(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing facing)
    {
        int direction = facing == Player.Facing.Right ? 1 : -1;
        //Vector2 force = direction * new Vector2(1, 1);

        Drop(attackerPosition, Vector2.zero);
    }

}
