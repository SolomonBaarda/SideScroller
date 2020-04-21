using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    private float initialSetup = 0.25f;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();

        col.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(InitialDisable());
    }



    private void PickUp(GameObject player)
    {
        col.enabled = false;

        player.GetComponent<Player>().PickedUpCoin();

        Destroy(gameObject);
    }



    private IEnumerator InitialDisable()
    {
        yield return new WaitForSeconds(initialSetup);
        col.enabled = true;
    }

}
