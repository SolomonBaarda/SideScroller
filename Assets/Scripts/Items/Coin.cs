using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : CollectableItem, IAmLoot
{
    public float initialSetupTime = DEFAULT_INITIAL_SETUP_TIME;

    new protected void Awake()
    {
        base.Awake();
        collideToPickUp = true;
    }


    private void Start()
    {
        DisableFor(initialSetupTime);
    }

}
