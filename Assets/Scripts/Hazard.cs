using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Player is colliding with this object 
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer(Player.PLAYER_LAYER)))
        {
            // Reference to player and controller script
            Player p = collision.GetComponentInParent<Player>();

            p.SetDead();
        }
    }
}
