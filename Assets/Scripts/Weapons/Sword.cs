using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon, IInteractable, ICanBeHeld
{
    public string Name => "Sword";

    public bool IsAttacking { get; private set; } = false;

    public Collider2D blade;

    public float AttackTimeSeconds => 2 * StabTimeSeconds;
    private const float StabTimeSeconds = 0.25f;
    private const float StabSpeed = 4f;

    public Transform HandlePosition;
    public Transform GroundPosition => HandlePosition;

    public WeaponPosition Position { get; set; } = WeaponPosition.Down;

    private Rigidbody2D rigid;



    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public List<GameObject> InAreaOfAttack()
    {
        // Pass parent game object - should be Player
        return PlayerInteraction.InAreaOfAttack(blade, transform.parent.gameObject);
    }


    private void AttackAllOnce(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        foreach (GameObject g in InAreaOfAttack())
        {
            if (WorldItem.ExtendsClass<ICanBeAttacked>(g))
            {
                // Attack all the items
                ICanBeAttacked a = (ICanBeAttacked)WorldItem.GetClass<ICanBeAttacked>(g);
                a.WasAttacked(attackerPosition, attackerVelocity);
            }
        }
    }


    private IEnumerator SwingSword(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        IsAttacking = true;

        // Attack forward
        float attackTimer = 0;
        while (attackTimer <= StabTimeSeconds)
        {
            // Attack everything 
            AttackAllOnce(attackerPosition, attackerVelocity);

            // Stab forward for next frame
            Vector2 currentPos = transform.position;
            currentPos.x += transform.parent.localScale.x * (StabSpeed * Time.deltaTime);
            transform.position = currentPos;

            // Update the timer
            attackTimer += Time.deltaTime;
            yield return null;
        }

        // Bring sword back (cooldown)
        attackTimer = 0;
        while (attackTimer <= StabTimeSeconds)
        {
            // Stab forward for next frame
            Vector2 currentPos = transform.position;
            currentPos.x -= transform.parent.localScale.x * (StabSpeed * Time.deltaTime);
            transform.position = currentPos;

            // Update the timer
            attackTimer += Time.deltaTime;
            yield return null;
        }

        IsAttacking = false;
    }


    public void Attack(Vector2 attackerPosition, Vector2 attackerVelocity)
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
        rigid.isKinematic = true;
        rigid.velocity = Vector2.zero;

        transform.parent = player.transform;
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;
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


}
