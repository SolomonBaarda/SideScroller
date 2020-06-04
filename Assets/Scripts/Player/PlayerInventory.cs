using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Inventories for simple items 
    private SimpleInventoryItem<Coin> coins;

    public GameObject HeldItem;

    private Player player;

    public void Awake()
    {
        player = GetComponent<Player>();

        coins = new SimpleInventoryItem<Coin>(0, int.MaxValue);
    }


    public bool CanDropHeldItem()
    {
        return HeldItem != null;
    }

    public void DropHeldItem()
    {
        if (CanDropHeldItem())
        {
            ICanBeHeld p = HeldItem.GetComponent<ICanBeHeld>();

            // Drop the payload
            p.Drop(player.Head.position, GetComponent<Rigidbody2D>().velocity);
            HeldItem = null;
        }
    }


    public bool PickUp(GameObject g)
    {
        // Item can be held
        if (WorldItem.ExtendsClass<ICanBeHeld>(g))
        {
            if (HeldItem == null)
            {
                // Hold the item
                ICanBeHeld h = (ICanBeHeld)WorldItem.GetClass<ICanBeHeld>(g);
                h.Hold(player, player.Head.localPosition);
                HeldItem = g;

                return true;
            }
            else
            {
                return false;
            }
        }
        // It is collectable
        if (WorldItem.ExtendsClass<ICollectable>(g))
        {
            // Coin
            if (WorldItem.ExtendsClass<Coin>(g))
            {
                Coin coin = (Coin)WorldItem.GetClass<Coin>(g);

                // Add another coin
                coins.total++;

                // Destroy it
                Destroy(coin.gameObject);

                return true;
            }
        }

        return false;
    }




    public void UpdateHeldItemPosition()
    {
        if (HeldItem != null)
        {
            ICanBeHeld h = (ICanBeHeld)WorldItem.GetClass<ICanBeHeld>(HeldItem);
            h.SetLocalPosition(player.Head.localPosition);
        }
    }


    public IWeapon GetPrimaryWeapon()
    {
        if (WorldItem.ExtendsClass<IWeapon>(HeldItem))
        {
            return (IWeapon)WorldItem.GetClass<IWeapon>(HeldItem);
        }

        else return null;
    }


    public IInventory<T> GetInventory<T>() where T : class
    {
        if (typeof(T).Equals(typeof(Coin)))
        {
            return (IInventory<T>)coins;
        }

        return null;
    }



    [System.Serializable]
    public class InventoryItem<T> where T : class
    {
        public GameObject worldItem;
        public T item;

        public InventoryItem(GameObject worldItem, T item)
        {
            this.worldItem = worldItem;
            this.item = item;
        }
    }


    [System.Serializable]
    public class SimpleInventoryItem<T> : IInventory<T> where T : class
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


    public interface IInventory<T> where T : class
    {
        int Total { get; }
        int Max { get; }
    }

}
