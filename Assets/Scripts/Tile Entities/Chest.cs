using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public enum ChestState { Locked, Closed, Open };
    public ChestState state;

    public enum ChestContents { Full, Empty };
    public ChestContents contents;

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




    public void Open()
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

    public void Close()
    {
        if (state.Equals(ChestState.Open))
        {
            state = ChestState.Closed;
        }
    }


    public void Unlock()
    {
        if (state.Equals(ChestState.Locked))
        {
            state = ChestState.Closed;
        }
    }
}
