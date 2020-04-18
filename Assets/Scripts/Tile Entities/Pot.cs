using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    public enum PotState { Whole, Broken };
    public PotState state;

    private void Start()
    {
        state = PotState.Whole;
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
                if(state.Equals(PotState.Whole))
                {
                    state = PotState.Broken;
                    Animator a = GetComponent<Animator>();
                    a.SetBool("isBroken", state.Equals(PotState.Broken));

                    Vector2 pos = new Vector2();
                    pos.x = transform.position.x;
                    pos.y = transform.position.y;

                    ItemManager.OnPotBroken.Invoke(pos);
                }

            }
        }
    }
}
