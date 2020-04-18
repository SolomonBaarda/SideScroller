using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float initialSetup = 0.25f;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }

    private void Start()
    {
        StartCoroutine(InitialDisable());
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        // Player is colliding with this object 
        if (collision.gameObject.layer == LayerMask.NameToLayer(Player.PLAYER))
        {
            // Reference to player and controller script
            Player p = collision.transform.root.GetComponent<Player>();

            p.PickedUpCoin();
            Destroy(gameObject);
        }
    }

    IEnumerator InitialDisable()
    {
        yield return new WaitForSeconds(initialSetup);
        GetComponent<BoxCollider2D>().enabled = true;
    }

}
