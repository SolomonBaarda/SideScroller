using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public bool isEnabled;

    private void Start()
    {
        isEnabled = false;
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
                Toggle();

            }
        }
    }

    public void Toggle()
    {
        isEnabled = !isEnabled;
        GetComponent<Animator>().SetBool("isEnabled", isEnabled);
    }
}
