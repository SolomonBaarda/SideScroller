using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public bool isEnabled;

    private void Start()
    {
        isEnabled = false;
    }



    public void Toggle()
    {
        isEnabled = !isEnabled;
        GetComponent<Animator>().SetBool("isEnabled", isEnabled);
    }
}
