using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public enum ChestState { Locked, Closed, Open };
    public ChestState state;

    // Start is called before the first frame update
    private void Start()
    {
        state = ChestState.Closed;
    }

    // Update is called once per frame
    private void Update()
    {
        Animator a = GetComponent<Animator>();
        //a.SetBool("isLocked", state.Equals(ChestState.Locked));
        a.SetBool("isOpen", state.Equals(ChestState.Open));
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        // Player is colliding with this object 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Reference to player and controller script
            Transform parent = collision.gameObject.transform.parent;
            Player p = parent.GetComponent<Player>();

            // Open the chest 
            if (Input.GetKeyDown(p.controller.keys.interact1))
            {
                if (state.Equals(ChestState.Closed))
                {
                    Open();
                }
                else if (state.Equals(ChestState.Open))
                {
                    Close();
                }

            }
        }
    }

    public void Open()
    {
        if (state.Equals(ChestState.Closed))
        {
            state = ChestState.Open;
            Debug.Log("open");
        }
    }

    public void Close()
    {
        if (state.Equals(ChestState.Open))
        {
            state = ChestState.Closed;
            Debug.Log("open");
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
