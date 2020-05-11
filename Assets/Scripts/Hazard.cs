using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // A player is colliding with this
        if (LayerMask.LayerToName(collision.gameObject.layer).Contains(Player.DEFAULT_PLAYER_LAYER))
        {
            // Ensure it is a valid collision
            if (!collision.isTrigger)
            {
                // Reference to player and controller script
                Player p = collision.GetComponentInParent<Player>();

                if (p != null)
                {
                    // Let the manager know that the player should be killed
                    PlayerManager.OnPlayerDie.Invoke(p);
                }
            }
        }
    }
}
