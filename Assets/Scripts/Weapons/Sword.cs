using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon
{
    public string Name => "Sword";

    public bool IsAttacking { get; private set; } = false;

    public Collider2D blade;

    public float AttackTimeSeconds => 0.5f;


    public List<GameObject> InAreaOfAttack()
    {
        return PlayerInteraction.InAreaOfAttack(blade, gameObject);
    }


    private void CheckAttack(Vector2 attackerPosition, Vector2 attackerVelocity)
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

        // Stab forward
        float attackTimer = 0;
        while (attackTimer <= AttackTimeSeconds)
        {
            CheckAttack(attackerPosition, attackerVelocity);

            attackTimer += Time.deltaTime;
            yield return null;
        }

        // Bring back
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
        if (!IsAttacking)
        {
            // Start attack coroutine
            StartCoroutine(SwingSword(attackerPosition, attackerVelocity));
        }
    }

}
