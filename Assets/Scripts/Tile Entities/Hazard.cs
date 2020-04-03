using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Player is colliding with this object 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Reference to player and controller script
            Transform parent = collision.gameObject.transform.parent;
            Player p = parent.GetComponent<Player>();

            p.SetDead();
        }
    }
}
