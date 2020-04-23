using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private enum ChestState { Locked, Closed, Open };
    [SerializeField] private ChestState state;

    private enum ChestContents { Full, Empty };
    [SerializeField] private ChestContents contents;

    private void Awake()
    {
        state = ChestState.Closed;
        contents = ChestContents.Full;

    }

    // Update is called once per frame
    private void Update()
    {
        Animator a = GetComponent<Animator>();
        //a.SetBool("isLocked", state.Equals(ChestState.Locked));
        a.SetBool("isOpen", state.Equals(ChestState.Open));
    }


    public  bool Interact()
    {
        if (state == ChestState.Locked)
        {
            Unlock();
        }
        else if (state == ChestState.Closed)
        {
            return Open();
        }
        else if (state == ChestState.Open)
        {
            Close();
        }

        return false;
    }


    public  bool PickUp(PlayerInventory player)
    {
        return false;
    }


    private bool Open()
    {
        if (state.Equals(ChestState.Closed))
        {
            state = ChestState.Open;
            if (contents.Equals(ChestContents.Full))
            {
                contents = ChestContents.Empty;
                return true;
            }
        }
        return false;
    }


    private void Close()
    {
        if (state.Equals(ChestState.Open))
        {
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


}
