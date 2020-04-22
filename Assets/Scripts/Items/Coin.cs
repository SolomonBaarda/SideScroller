using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : InteractableItem
{
    [SerializeField]
    private float initialSetup = 0.5f;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();

        col.enabled = false;

        canBePickedUp = true;
    }

    private void Start()
    {
        StartCoroutine(InitialDisable());
    }

    private IEnumerator InitialDisable()
    {
        yield return new WaitForSeconds(initialSetup);
        col.enabled = true;
    }

    public override bool Interact()
    {
        return false;
    }

    public override bool PickUp(Player player)
    {
        col.enabled = false;

        player.PickedUpCoin();

        Destroy(gameObject);

        return false;
    }
}
