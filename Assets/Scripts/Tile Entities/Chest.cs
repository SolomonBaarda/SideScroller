using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InteractableItem
{
    private enum ChestState { Locked, Closed, Open };
    [SerializeField]
    private ChestState state;

    private enum ChestContents { Full, Empty };
    [SerializeField]
    private ChestContents contents;

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


    public override void Interact()
    {
        
    }

    private void Open()
    {
        if (state.Equals(ChestState.Closed))
        {
            state = ChestState.Open;
            if (contents.Equals(ChestContents.Full))
            {
                contents = ChestContents.Empty;
            }
        }
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
