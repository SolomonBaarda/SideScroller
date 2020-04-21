using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    private enum PotState { Whole, Broken };
    private PotState state;

    private void Awake()
    {
        state = PotState.Whole;
    }


}
