using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Inventories for simple items 
    private SimpleInventoryItem<Coin> coins;

    public GameObject HeldItemLeft, HeldItemRight;

    private Player player;
    private Rigidbody2D rigid;

    public void Awake()
    {
        player = GetComponent<Player>();
        rigid = GetComponent<Rigidbody2D>();

        coins = new SimpleInventoryItem<Coin>(0, int.MaxValue);
    }


    public bool IsHoldingItems()
    {
        return HeldItemLeft != null || HeldItemRight != null;
    }

    public bool DropLeftHand()
    {
        if (HeldItemLeft != null)
        {
            ICanBeHeld p = HeldItemLeft.GetComponent<ICanBeHeld>();

            // Drop the item in the left hand
            p.Drop(HeldItemLeft.transform.position, rigid.velocity);
            HeldItemLeft = null;

            return true;
        }

        return false;
    }

    public bool DropRightHand()
    {
        if (HeldItemRight != null)
        {
            ICanBeHeld p = HeldItemRight.GetComponent<ICanBeHeld>();

            // Drop the item in right hand
            p.Drop(HeldItemRight.transform.position, rigid.velocity);
            HeldItemRight = null;

            return true;
        }

        return false;
    }

    public void DropAllHeldItems()
    {
        DropLeftHand();
        DropRightHand();
    }


    public bool PickUp(GameObject g)
    {
        // Item can be held
        if (WorldItem.ExtendsClass<ICanBeHeld>(g))
        {
            // Hold it in the left hand
            if (WorldItem.ExtendsClass<Payload>(g))
            {
                if (HeldItemLeft == null)
                {
                    // Hold the payload
                    ICanBeHeld h = (ICanBeHeld)WorldItem.GetClass<ICanBeHeld>(g);
                    g.transform.localScale = transform.localScale;
                    h.Hold(player);
                    h.SetHeldPosition(player.LeftHand);
                    HeldItemLeft = g;

                    return true;
                }
                return false;
            }
            else
            {
                if (HeldItemRight == null)
                {
                    // Check the item is not already being held
                    ICanBeHeld h = (ICanBeHeld)WorldItem.GetClass<ICanBeHeld>(g);
                    if (!h.IsBeingHeld)
                    {
                        // pick up the item
                        g.transform.localScale = transform.localScale;
                        h.Hold(player);
                        h.SetHeldPosition(player.RightHand);
                        HeldItemRight = g;

                        return true;
                    }
                }
            }
            return false;
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
        if (HeldItemLeft != null)
        {
            ICanBeHeld h = (ICanBeHeld)WorldItem.GetClass<ICanBeHeld>(HeldItemLeft);
            h.SetHeldPosition(player.LeftHand);
        }
        if (HeldItemRight != null)
        {
            ICanBeHeld h = (ICanBeHeld)WorldItem.GetClass<ICanBeHeld>(HeldItemRight);
            h.SetHeldPosition(player.RightHand);
        }
    }


    public IWeapon GetPrimaryWeapon()
    {
        if (HeldItemRight != null && WorldItem.ExtendsClass<IWeapon>(HeldItemRight))
        {
            return (IWeapon)WorldItem.GetClass<IWeapon>(HeldItemRight);
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
