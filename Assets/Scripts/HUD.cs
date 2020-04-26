using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    public static UnityAction<HUD> OnHUDLoaded;

    public TMP_Text text_coin_count;
    public TMP_Text text_game_time;

    public bool show_fps = true;
    public TMP_Text text_fps;


    private void Awake()
    {
        SetHUDStyleTMPro(ref text_coin_count);
        SetHUDStyleTMPro(ref text_game_time);

        SetHUDStyleTMPro(ref text_fps);

        SceneManager.sceneLoaded += HUDLoaded;
    }


    private void HUDLoaded(Scene s, LoadSceneMode l)
    {
        OnHUDLoaded.Invoke(this);
    }


    public void UpdateHUD(HUDElements elements)
    {
        text_coin_count.text = elements.coin_count.ToString();
        // Display time as 1dp only
        text_game_time.text = elements.game_time.ToString("0.0");

        if (show_fps)
        {
            text_fps.enabled = true;
            text_fps.text = elements.fps.ToString("0.0");
        }
        else
        {
            text_fps.enabled = false;
        }
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
        public int player_health;
        public int player_max_health;
        public float game_time;
        public float fps;

        public HUDElements(int coin_count, int player_health, int player_max_health, float game_time, float fps)
        {
            this.coin_count = coin_count;
            this.player_health = player_health;
            this.player_max_health = player_max_health;
            this.game_time = game_time;
            this.fps = fps;
        }
    }
}
