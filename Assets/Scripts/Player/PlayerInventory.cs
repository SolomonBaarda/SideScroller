using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private SimpleInventoryItem<Coin> coins;

    [SerializeField]
    private InventoryItem<Weapon> weapon;

    [SerializeField]
    private List<InventoryItem<Buff>> buffs;
    [SerializeField]
    private Buff currentTotal;

    public void Awake()
    {
        buffs = new List<InventoryItem<Buff>>();
    }




    public void DropWeapon(bool drop)
    {
        if (drop)
        {
            if (weapon.worldItem != null && weapon.item != null)
            {
                // Set them to null
                weapon.item = null;
                weapon.worldItem.Drop(transform.position, Vector2.zero);
                weapon.worldItem = null;
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

            // Coin
            Coin coin = (Coin)c;
            if (coin != null)
            {
                // Add another coin
                coins.total++;

                // Destroy it
                Destroy(coin.gameObject);

                return true;
            }
            // Weapon
            Weapon w = (Weapon)item;
            if (w != null)
            {
                if (weapon.item == null)
                {
                    weapon.worldItem = c;
                    weapon.item = w;
                    return true;
                }
            }

            // Buff
            Buff b = (Buff)item;
            if (WorldItem.ImplementsInterface<Buff>(g))
            {
                return PickUpBuff(c, b);
            }
        }

        return false;
    }



    public int GetCoinCount()
    {
        return coins.total;
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




    private bool PickUpBuff(CollectableItem worldItem, Buff buff)
    {
        InventoryItem<Buff> b = new InventoryItem<Buff>();
        b.worldItem = worldItem;
        b.item = buff;

        buffs.Add(b);
        UpdateCurrentBuffTotal();
        return true;

    }


    private void UpdateCurrentBuffTotal()
    {
        // Create a new buff instance
        Buff total = ScriptableObject.CreateInstance<Buff>();

        // Combine all the values 
        foreach (InventoryItem<Buff> b in buffs)
        {
            total.CombineBuffs(b.item);
        }

        // Update the current value
        currentTotal = total;
    }


    public Buff GetCurrentTotal()
    {
        return currentTotal;
    }





    [System.Serializable]
    public struct InventoryItem<T> where T : class
    {
        public CollectableItem worldItem;
        public T item;
    }


    [System.Serializable]
    public struct SimpleInventoryItem<T> where T : class
    {
        public int total;
    }

}
