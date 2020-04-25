using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class HUD : MonoBehaviour
{
    public static UnityAction<HUDElements> OnUpdateHUD;

    public TMP_Text text_coin_count;
    public TMP_Text text_game_time;


    private void Awake()
    {
        SetHUDStyleTMPro(ref text_coin_count);
        SetHUDStyleTMPro(ref text_game_time);

        OnUpdateHUD += UpdateHUD;
    }



    private void UpdateHUD(HUDElements elements)
    {
        text_coin_count.text = elements.coin_count.ToString();
        // Display time as 1dp only
        text_game_time.text = elements.game_time.ToString("0.0");
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


    public struct HUDElements
    {
        public int coin_count;
        public float game_time;
    }
}
