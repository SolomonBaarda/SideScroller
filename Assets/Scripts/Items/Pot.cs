using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable, ILoot, ICanBeAttacked, ICanBeHeld
{
    public LootTable table;

    [SerializeField]
    private int inventory_size = 1;

    private bool hasContents = true;

    private Rigidbody2D rigid;

    public Transform groundPosition;

    public Transform GroundPosition { get { return groundPosition; } }

    new private void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
    }


    public void Interact(PlayerInventory inventory)
    {
        if(inventory.PickUp(gameObject))
        {
            Hold(inventory.gameObject, inventory.GetComponent<Player>().Head.localPosition);
        }
    }


    public LootTable GetLootTable()
    {
        return table;
    }


    public int GetTotalItemsToBeLooted()
    {
        return inventory_size;
    }

    public bool IsLootable()
    {
        return hasContents;
    }

    public void Loot()
    {
        hasContents = false;
    }

    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        InteractionManager.OnPlayerInteractWithItem(gameObject, null);

        // Break the pot
        Animator a = GetComponent<Animator>();
        a.SetTrigger("Break");
    }

    public void Hold(GameObject player, Vector2 localPosition)
    {
        transform.parent = player.transform;
        transform.localPosition = localPosition;

        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;

        trigger.enabled = false;
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;

        trigger.enabled = true;
    }

    public void SetLocalPosition(Vector2 local)
    {
        local.y += Mathf.Abs(GroundPosition.localPosition.y);
        transform.localPosition = local;
    }
}
