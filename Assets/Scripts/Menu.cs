using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;


public class Menu : MonoBehaviour
{
    public TMP_Text play_button;
    public static UnityAction OnMenuClose;

    // Start is called before the first frame update
    private void Start()
    {
        SetMenuText(play_button);

        // Load the main game
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        // Add the event calls and functions
        OnMenuClose += Unload;
        play_button.GetComponent<Button>().onClick.AddListener(OnMenuClose);
    }

    private void Unload()
    {
        SceneManager.UnloadSceneAsync(0);
        GameManager.OnGameStart.Invoke();
    }

    private void SetMenuText(TMP_Text t)
    {
        Button b = t.GetComponent<Button>();
        if (b != null)
        {
            ColorBlock bl = ColorBlock.defaultColorBlock;

            bl.normalColor = Color.white;
            bl.highlightedColor = Color.gray;
            bl.pressedColor = Color.gray;

            b.colors = bl;
        }
        else
        {
            t.color = Color.white;
        }
    }

}
