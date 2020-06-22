using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour, IWeapon, IInteractable, ICanBeHeld
{
    public const string DisplayName = "Shuriken";
    public string Name => DisplayName;

    public bool IsAttacking => ThrownBladesParent.childCount > 0;

    public bool WasBlocked => false;

    private Rigidbody2D rigid;

    public List<GameObject> AreaOfAttack
    {
        get
        {
            // Loop through each blade and add all the collisions
            List<GameObject> blades = new List<GameObject>();
            foreach (ShurikenBlade s in ThrownBlades)
            {
                foreach (GameObject g in s.AreaOfAttack)
                {
                    if (!blades.Contains(g))
                    {
                        blades.Add(g);
                    }
                }

            }
            return blades;
        }
    }

    public bool IsBeingHeld { get; private set; } = false;

    public Transform handlePosition;
    public Transform GroundPosition => handlePosition;


    [Header("Blades")]
    public GameObject shurikenBladePrefab;

    public Transform ThrownBladesParent;
    private ShurikenBlade[] ThrownBlades => ThrownBladesParent.GetComponentsInChildren<ShurikenBlade>();


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }



    public void Attack(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing facing, ref GameObject inventoryReferenceToThisWeapon)
    {
        ThrowNewShuriken(attackerPosition, attackerVelocity, facing);
    }


    private void ThrowNewShuriken(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing facing)
    {
        GameObject blade = Instantiate(shurikenBladePrefab, ThrownBladesParent);
        ShurikenBlade b = blade.GetComponent<ShurikenBlade>();
        StartCoroutine(b.Throw(attackerPosition, attackerVelocity, facing));
    }


    public void AttackWasBlocked()
    {

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

        transform.parent = ItemManager.StaticItemParent;
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
}
