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

    public GameObject payload;

    private Player player;

    public void Awake()
    {
        player = GetComponent<Player>();

        buffs = new List<InventoryItem<Buff>>();

        health = new SimpleInventoryItem<Health>(10, 10);
        coins = new SimpleInventoryItem<Coin>(0, int.MaxValue);
    }


    public void DropAllItems()
    {
        DropPayload();
    }

    public bool CanDropPayload()
    {
        return payload != null;
    }

    public void DropPayload()
    {
        if (CanDropPayload())
        {
            Payload p = payload.GetComponent<Payload>();

            // Drop the payload
            p.Drop(player.Head.position, GetComponent<Rigidbody2D>().velocity);
            payload = null;
        }
    }


    public bool PickUp(GameObject g)
    {
        // It is collectable
        if (WorldItem.ExtendsClass<ICollectable>(g))
        {
            CollectableItem c = (CollectableItem)WorldItem.GetClass<CollectableItem>(g);
            ItemBase item = c.item;

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
            // Payload
            else if (WorldItem.ExtendsClass<Payload>(g))
            {
                Payload p = (Payload)WorldItem.GetClass<Payload>(g);
                payload = g;
                
                p.PickUp(gameObject, player.Head.position);
            }
            // Weapon
            else if (item is Weapon w)
            {
                if (weapon.item == null)
                {
                    weapon.worldItem = c;
                    weapon.item = w;
                    return true;
                }
            }
            // Buff
            else if (item is Buff b)
            {
                CombineBuff(c, b);
                Destroy(c.gameObject);
                return true;
            }
        }

        return false;
    }




    public void UpdateHoldingPayloadPosition()
    {
        // Check if player has payload in inventory
        Payload p = GetComponentInChildren<Payload>();
        if (p != null)
        {
            payload = p.gameObject;

            // Set the correct local position
            p.SetLocalPosition(player.Head.localPosition);
        }
        else
        {
            payload = null;
        }
    }


    private void CombineBuff(CollectableItem worldItem, Buff buff)
    {
        if (currentTotal == null)
        {
            // Create a new buff instance
            currentTotal = ScriptableObject.CreateInstance<Buff>();
        }

        // Combine the buff
        currentTotal.CombineBuffs(buff);
    }




    public IInventory<T> GetInventory<T>() where T : class
    {
        if (typeof(T).Equals(typeof(Health)))
        {
            return (IInventory<T>)health;
        }
        else if (typeof(T).Equals(typeof(Coin)))
        {
            return (IInventory<T>)coins;
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
