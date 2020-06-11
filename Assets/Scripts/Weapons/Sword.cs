using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon, IInteractable, ICanBeHeld, IBlock
{
    public string Name => "Sword";

    public bool IsAttacking { get; private set; } = false;
    public bool WasBlocked { get; private set; } = false;

    public Collider2D blade;

    public float AttackTimeSeconds => 2 * StabTimeSeconds;
    private const float StabTimeSeconds = 0.25f;
    private const float StabSpeed = 4f;

    public Transform HandlePosition;
    public Transform GroundPosition => HandlePosition;

    public bool IsBeingHeld { get; private set; } = false;

    private Rigidbody2D rigid;


    public List<GameObject> AreaOfAttack
    {
        get
        {
            // Pass parent game object - should be Player
            return PlayerInteraction.InAreaOfAttack(blade, transform.parent.gameObject);
        }
    }


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }


    private void TryAttackAllOnce(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        // Ensure the sword hasn't been blocked first
        if (!WasBlocked)
        {
            foreach (GameObject g in AreaOfAttack)
            {
                // The object can be attacked
                if (WorldItem.ExtendsClass<ICanBeAttacked>(g))
                {
                    // Attack all the items
                    ICanBeAttacked a = (ICanBeAttacked)WorldItem.GetClass<ICanBeAttacked>(g);
                    a.WasAttacked(attackerPosition, attackerVelocity, (IWeapon)WorldItem.GetClass<IWeapon>(gameObject));
                }
            }
        }
    }


    private IEnumerator SwingSword(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        IsAttacking = true;
        WasBlocked = false;
        Vector2 originalPosition = transform.localPosition;

        // Attack forward
        float attackTimer = 0;
        while (attackTimer <= StabTimeSeconds && !WasBlocked)
        {
            // Attack everything 
            TryAttackAllOnce(attackerPosition, attackerVelocity);

            // Break out if was just blocked
            if(WasBlocked)
            {
                break;
            }

            // Stab forward for next frame
            Vector2 currentPos = transform.localPosition;
            currentPos.x += StabSpeed * Time.deltaTime;
            transform.localPosition = currentPos;

            // Update the timer
            attackTimer += Time.deltaTime;
            yield return null;
        }

        // Bring sword back (cooldown)
        attackTimer = 0;
        while (attackTimer <= StabTimeSeconds && transform.localPosition.x > originalPosition.x)
        {
            // Bring the sword back
            Vector2 currentPos = transform.localPosition;
            currentPos.x -= StabSpeed * Time.deltaTime;
            transform.localPosition = currentPos;

            // Update the timer
            attackTimer += Time.deltaTime;
            yield return null;
        }

        IsAttacking = false;
    }


    public void Attack(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing _)
    {
        // Only attack if not already doing so
        if (!IsAttacking)
        {
            // Start attack coroutine
            StartCoroutine(SwingSword(attackerPosition, attackerVelocity));
        }
    }


    public void Hold(Player player)
    {
        IsBeingHeld = true;

        rigid.isKinematic = true;
        rigid.velocity = Vector2.zero;

        transform.parent = player.transform;

        StopAllCoroutines();
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        IsBeingHeld = false;

        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;

        // Destroy the sword if it collides with a hazard
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(Hazard.LAYER));
        filter.useTriggers = true;
        StartCoroutine(WorldItem.WaitForThenInvoke(gameObject, filter, Destroy));
    }


    private void Destroy()
    {
        Destroy(gameObject);
    }


    public void SetHeldPosition(Transform t)
    {
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        // Only update the position if the weapon is not attacking 
        if (!IsAttacking)
        {
            transform.rotation = Quaternion.identity;
            transform.localPosition = (Vector2)t.localPosition + -(Vector2)GroundPosition.localPosition;
        }
    }


    public bool Interact(Player player)
    {
        return player.Inventory.PickUp(gameObject);
    }


    public bool DidBlock(IWeapon weapon)
    {
        // Parry the sword
        if (weapon is Sword)
        {
            return true;
        }
        return false;
    }


    public void AttackWasBlocked()
    {
        WasBlocked = true;
    }


    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity, IWeapon weapon)
    {
        if(DidBlock(weapon))
        {
            // Block attack from weapon and also cancel this attack if it was going on
            weapon.AttackWasBlocked();
            AttackWasBlocked();
        }
    }
}
