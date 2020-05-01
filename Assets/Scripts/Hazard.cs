using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // A player is colliding with this
        if (LayerMask.LayerToName(collision.gameObject.layer).Contains(Player.DEFAULT_PLAYER_LAYER))
        {
            // Reference to player and controller script
            Player p = collision.GetComponentInParent<Player>();

            if (p.IsAlive)
            {
                p.SetDead();
                Debug.Log(p.PLAYER_LAYER + " has died.");
            }
        }
    }
}
