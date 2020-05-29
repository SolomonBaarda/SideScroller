using UnityEngine;

public class Chest : WorldItem, IInteractable, ILootable
{
    private enum ChestState { Locked, Closed, Open };
    [SerializeField] private ChestState state = ChestState.Closed;

    private enum ChestContents { Full, Empty };
    [SerializeField] private ChestContents contents = ChestContents.Full;

    [SerializeField]
    private int inventory_size = 3;

    public LootTable table;

    private Animator a;

    new private void Awake()
    {
        base.Awake();

        a = GetComponent<Animator>();
    }



    private void Open()
    {
        if (state.Equals(ChestState.Closed))
        {
            state = ChestState.Open;
            a.SetTrigger("Open");
        }
    }


    private void Close()
    {
        if (state.Equals(ChestState.Open))
        {
            a.SetTrigger("Close");
            state = ChestState.Closed;
        }
    }

    private void Unlock()
    {
        if (state.Equals(ChestState.Locked))
        {
            state = ChestState.Closed;
        }
    }

    public void Interact(PlayerInventory inventory)
    {
        if (state == ChestState.Locked)
        {
            Unlock();
        }
        else if (state == ChestState.Closed)
        {
            Open();
        }
        else if (state == ChestState.Open)
        {
            Close();
        }
    }

    public LootTable GetLootTable()
    {
        return table;
    }

    public int GetTotalItemsToBeLooted()
    {
        return inventory_size;
    }


    public bool IsLootable()
    {
        return contents.Equals(ChestContents.Full);
    }

    public void Loot()
    {
        if (contents.Equals(ChestContents.Full))
        {
            contents = ChestContents.Empty;
        }
    }

}
