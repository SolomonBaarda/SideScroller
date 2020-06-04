using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Sword : MonoBehaviour, IWeapon, IInteractable, ICanBeHeld
{
    public string Name => "Sword";

    public bool IsAttacking { get; private set; } = false;

    public Collider2D blade;

    public float AttackTimeSeconds => 0.5f;

    public Transform GroundPosition => throw new System.NotImplementedException();

    private Rigidbody2D rigid;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

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
        Debug.Log("starting");
        IsAttacking = true;

        // Stab forward
        float attackTimer = 0;
        while (attackTimer <= AttackTimeSeconds)
        {
            CheckAttack(attackerPosition, attackerVelocity);
            attackTimer += Time.deltaTime;
            Debug.Log(attackTimer);
            yield return null;
        }

        Debug.Log("finished attacking");
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


    public void Hold(Player player, Vector2 localPosition)
    {
        rigid.isKinematic = true;
        rigid.velocity = Vector2.zero;

        transform.parent = player.transform;
        transform.localPosition = localPosition;
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
        transform.localPosition = local;
    }

    public void Interact(Player player)
    {
        if(player.Inventory.PickUp(gameObject))
        {
            Hold(player, player.Head.localPosition);
        }
    }
}
