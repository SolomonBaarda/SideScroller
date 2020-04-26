using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Inventories for simple items 
    private SimpleInventoryItem<Health> health;
    private SimpleInventoryItem<Coin> coins;

    // Complex items
    [SerializeField] private InventoryItem<Weapon> weapon;

    [SerializeField] private List<InventoryItem<Buff>> buffs;
    [SerializeField] private Buff currentTotal;

    public void Awake()
    {
        buffs = new List<InventoryItem<Buff>>();

        health = new SimpleInventoryItem<Health>(10, 10);
        coins = new SimpleInventoryItem<Coin>(0, int.MaxValue);
    }




    public void DropWeapon(bool drop)
    {
        if (drop)
        {
            if (weapon != null)
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
    }


    public bool PickUp(GameObject g)
    {
        // It is collectable
        if (WorldItem.ImplementsInterface<ICollectable>(g))
        {
            CollectableItem c = (CollectableItem)WorldItem.GetScriptThatImplements<CollectableItem>(g);
            ItemBase item = c.item;

            // Coin
            if (WorldItem.ImplementsInterface<Coin>(g))
            {
                Coin coin = (Coin)WorldItem.GetScriptThatImplements<Coin>(g);

                // Add another coin
                coins.total++;

                // Destroy it
                Destroy(coin.gameObject);

                return true;
            }
            // Weapon
            else if (item is Weapon)
            {
                Weapon w = (Weapon)item;

                if (weapon.item == null)
                {
                    weapon.worldItem = c;
                    weapon.item = w;
                    return true;
                }
            }
            // Buff
            else if (item is Buff)
            {
                Buff b = (Buff)item;
                return PickUpBuff(c, b);
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




    private bool PickUpBuff(CollectableItem worldItem, Buff buff)
    {
        InventoryItem<Buff> b = new InventoryItem<Buff>(worldItem, buff);

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




    public Inventory<T> GetInventory<T>() where T : class
    {
        if (typeof(T).Equals(typeof(Health)))
        {
            return (Inventory<T>)health;
        }
        else if (typeof(T).Equals(typeof(Coin)))
        {
            return (Inventory<T>)coins;
        }

        return null;
    }



    [System.Serializable]
    public class InventoryItem<T> where T : class
    {
        public CollectableItem worldItem;
        public T item;

        public InventoryItem(CollectableItem worldItem, T item)
        {
            this.worldItem = worldItem;
            this.item = item;
        }
    }


    [System.Serializable]
    public class SimpleInventoryItem<T> : Inventory<T> where T : class
    {
        public int total;
        public int max;
        public T item;

        public SimpleInventoryItem(int total, int max)
        {
            this.total = total;
            this.max = max;
        }

        public int Total => total;
        public int Max => max;
    }


    public interface Inventory<T> where T : class
    {
        int Total { get; }
        int Max { get; }
    }

}
