using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private int coins_collected = 0;

    [SerializeField]
    private int inventory_weapon_size = 3;
    private Weapon[] weapons;

    private List<Buff> buffs;
    private Buff currentTotal;

    public void Awake()
    {
        weapons = new Weapon[inventory_weapon_size];

        buffs = new List<Buff>();
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




    public bool PickUpBuff(Buff buff)
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
