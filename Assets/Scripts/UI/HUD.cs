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

    private Canvas canvas;

    public TMP_Text text_game_time;

    [Header("Coins")]
    public GameObject coin_display_parent;
    public TMP_Text text_coin_count;

    [Header("Health")]
    public Sprite image_health_full;
    public Sprite image_health_empty;

    [Header("FPS")]
    public bool show_fps = true;
    public TMP_Text text_fps;

    [Header("Quit")]
    public TMP_Text button_quit;

    [Header("Scoreboard")]
    public GameObject scoreboard;
    public GameObject scoreboardRowsParent;

    public GameObject scoreboardRowPrefab;
    private List<GameObject> scoreboardRows = new List<GameObject>();


    private SceneLoader sceneLoader;


    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        sceneLoader = SceneLoader.Instance;

        SetHUDStyleTMPro(ref text_coin_count);
        SetHUDStyleTMPro(ref text_game_time);

        SetHUDStyleTMPro(ref text_fps);

        SetHUDStyleTMPro(ref button_quit);

        SceneManager.sceneLoaded += HUDLoaded;
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HUDLoaded;
    }

    private void HUDLoaded(Scene s, LoadSceneMode l)
    {
        if(s.name.Equals(SceneLoader.HUD_SCENE))
        {
            OnHUDLoaded.Invoke(this);
        }
    }


    public void SetMultiplayer()
    {
        coin_display_parent.SetActive(false);
    }



    public void QuitToMenu()
    {
        sceneLoader.UnloadGameToMenu();
    }


    public void UpdateHUD(in HUDElements elements)
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


    public void SetVisible(bool enabled)
    {
        canvas.enabled = enabled;
    }



    public void ShowScoreboard(bool show)
    {
        scoreboard.SetActive(show);
    }

    public void UpdateScoreboardStats(GameManager.GameStats stats)
    {
        if(scoreboardRows.Count != stats.Players.Count)
        {
            // Add extra rows to the scoreboard if we need
            while (scoreboardRows.Count < stats.Players.Count)
            {
                scoreboardRows.Add(Instantiate(scoreboardRowPrefab, scoreboardRowsParent.transform));
            }
            // Remove excess rows if there are any
            while (scoreboardRows.Count > stats.Players.Count)
            {
                GameObject toRemove = scoreboardRows[0];
                scoreboardRows.RemoveAt(0);
                Destroy(toRemove);
            }
        }


        // Loop through each row in the scoreboard
        for(int i = 0; i < scoreboardRows.Count; i++)
        {
            GameManager.GameStats.PlayerStats player = stats.Players[i];

            // Set the player's stats
            ScoreboardRow r = scoreboardRows[i].GetComponent<ScoreboardRow>();
            r.SetColumnText(r.columns[0], player.name);
            r.SetColumnText(r.columns[1], player.deaths.ToString());
            r.SetColumnText(r.columns[2], player.coins.ToString());
        }
    }



    public readonly struct HUDElements
    {
        public readonly int coin_count;
        public readonly float game_time;
        public readonly float fps;

        public HUDElements(int coin_count, float game_time, float fps)
        {
            this.coin_count = coin_count;
            this.game_time = game_time;
            this.fps = fps;
        }
    }
}
