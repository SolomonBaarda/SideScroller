using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable, ILoot, ICanBeAttacked, ICanBeHeld
{
    public LootTable table;

    [SerializeField]
    private bool hasContents = true;

    private Rigidbody2D rigid;

    public Transform groundPosition;

    public Transform GroundPosition { get { return groundPosition; } }

    public bool IsBeingHeld { get; private set; } = false;


    //string IWeapon.Name => "Pot";

    public bool IsAttacking => false;

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
        InteractionManager.OnInteractWithItem(gameObject);

        // Break the pot
        Animator a = GetComponent<Animator>();
        a.SetTrigger("Break");
    }

    private IEnumerator WaitForCollisionWhenThrown()
    {
        Collider2D[] allColliders = GetComponents<Collider2D>();

        // Set the layermask to be ground and hazard
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(Hazard.LAYER, GroundCheck.GROUND_LAYER));
        filter.useTriggers = true;

        bool hasCollided = false;

        while (!hasCollided)
        {
            // Check each collider
            foreach (Collider2D c in allColliders)
            {
                // Check if there has been at least one collision
                if (Physics2D.OverlapCollider(c, filter, new Collider2D[1]) > 0)
                {
                    // Destroy the GameObject and exit
                    hasCollided = true;
                    Break();
                    yield break;
                }
            }

            yield return null;
        }
    }


    public void Hold(Player player)
    {
        IsBeingHeld = true;

        transform.parent = player.transform;

        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;

        trigger.enabled = false;
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        IsBeingHeld = false;

        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;

        trigger.enabled = true;

        StartCoroutine(WaitForCollisionWhenThrown());
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

    public void Attack(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        Drop(attackerPosition, attackerVelocity);
    }
}
