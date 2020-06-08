using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon, IInteractable, ICanBeHeld
{
    public string Name => "Sword";

    public bool IsAttacking { get; private set; } = false;

    public Collider2D blade;

    public float AttackTimeSeconds => 0.5f;

    public Transform HandlePosition;
    public Transform GroundPosition => HandlePosition;

    public WeaponPosition Position { get; private set; } = WeaponPosition.Down;

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
        while (attackTimer <= AttackTimeSeconds)
        {
            AttackAllOnce(attackerPosition, attackerVelocity);

            attackTimer += Time.deltaTime;
            yield return null;
        }

        // Bring sword back (cooldown)
        attackTimer = 0;
        while (attackTimer <= AttackTimeSeconds)
        {
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


    public void Hold(Player player, Vector2 localPosition)
    {
        rigid.isKinematic = true;
        rigid.velocity = Vector2.zero;

        transform.parent = player.transform;
        SetLocalPosition(localPosition);
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;
    }

    public void SetLocalPosition(Vector2 local)
    {
        transform.rotation = Quaternion.identity;
        transform.localPosition = local + -(Vector2)GroundPosition.localPosition;
    }

    public bool Interact(Player player)
    {
        return player.Inventory.PickUp(gameObject);
    }

    public bool MoveWeapon(WeaponPosition direction)
    {
        switch (Position)
        {
            case WeaponPosition.Up:
                switch (direction)
                {
                    // Can't move up as already up
                    case WeaponPosition.Up:
                        return false;
                    // Move down
                    case WeaponPosition.Down:
                        Position = WeaponPosition.Down;
                        return true;
                }
                break;
            case WeaponPosition.Down:
                switch (direction)
                {
                    // Move up
                    case WeaponPosition.Up:
                        Position = WeaponPosition.Up;
                        return true;
                    // Can't move down as already down
                    case WeaponPosition.Down:
                        return false;
                }
                break;
        }
        return false;
    }
}
