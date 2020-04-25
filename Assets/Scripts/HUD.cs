using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class HUD : MonoBehaviour
{
    public static UnityEvent<int> OnUpdatePlayerCoinCount;



    public TMP_Text text_coin_count;


    private void Awake()
    {
        SetHUDStyleTMPro(ref text_coin_count);

        OnUpdatePlayerCoinCount += UpdateCoinLabel;
    }



    private void UpdateCoinLabel(int value)
    {
        text_coin_count.text = value.ToString();
    }


    public static void SetHUDStyleTMPro(ref TMP_Text t)
    {
        // Button colours
        Button b = t.GetComponent<Button>();
        if (b != null)
        {
            ColorBlock bl = ColorBlock.defaultColorBlock;

            bl.normalColor = Color.white;
            bl.highlightedColor = Color.gray;
            bl.pressedColor = Color.gray;

            b.colors = bl;
        }
        // Text colours
        else
        {
            t.color = Color.white;
        }
    }

}
