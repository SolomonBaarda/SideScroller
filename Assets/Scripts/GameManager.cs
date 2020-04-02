using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static UnityAction OnGameStart;


    private void StartGame()
    {
        OnGameStart.Invoke();
    }


}
