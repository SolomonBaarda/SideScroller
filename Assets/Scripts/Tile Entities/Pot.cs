using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : InteractableItem
{
    private enum PotState { Whole, Broken };
    private PotState state;

    private void Awake()
    {
        state = PotState.Whole;
    }

    public override void Interact()
    {
        state = PotState.Broken;
        Animator a = GetComponent<Animator>();
        a.SetBool("isBroken", state.Equals(PotState.Broken));

        Vector2 pos = new Vector2();
        pos.x = transform.position.x;
        pos.y = transform.position.y;
    }

}
