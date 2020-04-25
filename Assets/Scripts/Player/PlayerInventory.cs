using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private int coins_collected = 0;

    [SerializeField]
    private int inventory_weapon_size = 1;
    private CollectableItem[] weaponItems;
    private Weapon[] weapons;

    private List<Buff> buffs;
    private Buff currentTotal;

    public void Awake()
    {
        weaponItems = new CollectableItem[inventory_weapon_size];
        weapons = new Weapon[inventory_weapon_size];

        buffs = new List<Buff>();
    }




    public void DropItem(bool drop)
    {
        if (drop)
        {
            if (weapons[0] != null)
            {
                weapons[0] = null;
                weaponItems[0].Drop(transform.position, Vector2.zero);
                weaponItems[0] = null;
            }
        }
    }


    public bool PickUp(GameObject g)
    {
        // It is collectable
        if (WorldItem.ImplementsInterface<ICollectable>(g))
        {
            CollectableItem c = (CollectableItem)WorldItem.GetScriptThatImplements<CollectableItem>(g);
            ItemBase item = c.item;

            // Weapon
            Weapon w = (Weapon)item;
            if (w != null)
            {
                if (weapons[0] == null)
                {
                    weaponItems[0] = c;
                    weapons[0] = w;
                    return true;
                }
            }

            // Buff
            Buff b = (Buff)item;
            if (WorldItem.ImplementsInterface<Buff>(g))
            {
                PickUpBuff(b);
                return true;
            }
        }

        return false;
    }





    public void Attack(float cycleWeapons, bool usePrimary, bool useSecondary, Vector2 playerVelocity)
    {

    }


    private void CycleWeapons(float direction)
    {

    }



    private void UsePrimary(bool isUsing)
    {

    }


    private void UseSecondary(bool isUsing)
    {

    }




    private bool PickUpBuff(Buff buff)
    {
        if (!buffs.Contains(buff))
        {
            buffs.Add(buff);
            UpdateCurrentBuffTotal();
            return true;
        }

        return false;
    }


    private void UpdateCurrentBuffTotal()
    {
        // Create a new buff instance
        Buff total = ScriptableObject.CreateInstance<Buff>();

        // Combine all the values 
        foreach (Buff b in buffs)
        {
            total.CombineBuffs(b);
        }

        // Update the current value
        currentTotal = total;
    }


    public Buff GetCurrentTotal()
    {
        return currentTotal;
    }


    public void PickUpCoin()
    {
        coins_collected++;
    }








}
