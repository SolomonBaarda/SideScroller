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

    public GameObject hud_root;

    [Header("FPS")]
    public bool show_fps = true;
    public TMP_Text text_fps;

    [Header("Quit Button")]
    public Button button_quit;

    [Header("Scoreboard")]
    public GameObject scoreboard;
    public GameObject scoreboardRowsParent;

    public GameObject scoreboardRowPrefab;
    private List<GameObject> scoreboardRows = new List<GameObject>();

    [Space]
    public TMP_Text text_game_time;


    private SceneLoader sceneLoader;


    private void Awake()
    {
        sceneLoader = SceneLoader.Instance;

        button_quit.onClick.AddListener(QuitToMenu);

        SceneManager.sceneLoaded += HUDLoaded;
    }


    private void OnDestroy()
    {
        button_quit.onClick.RemoveAllListeners();

        SceneManager.sceneLoaded -= HUDLoaded;
    }

    private void HUDLoaded(Scene s, LoadSceneMode l)
    {
        if (s.name.Equals(SceneLoader.HUD_SCENE))
        {
            OnHUDLoaded.Invoke(this);
        }
    }


    public void QuitToMenu()
    {
        sceneLoader.UnloadGameToMenu();
    }


    public void SetVisible(bool enabled)
    {
        hud_root.SetActive(enabled);
    }


    public void SetScoreboardVisible(bool show)
    {
        scoreboard.SetActive(show);
    }

    public void UpdateScoreboardStats(GameManager.GameStats stats)
    {
        // Set the correct number of rows
        if (scoreboardRows.Count != stats.Players.Count)
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
        for (int i = 0; i < scoreboardRows.Count; i++)
        {
            GameManager.GameStats.PlayerStats player = stats.Players[i];

            // Set the player's stats
            ScoreboardRow r = scoreboardRows[i].GetComponent<ScoreboardRow>();
            r.SetColumnText(r.columns[0], player.name);
            r.SetColumnText(r.columns[1], player.deaths.ToString());
            r.SetColumnText(r.columns[2], player.coins.ToString());
        }

        // Set the game time
        text_game_time.text = stats.game_time_seconds.ToString("0.0");

        // Display fps to 1dp
        text_fps.enabled = show_fps;
        text_fps.text = stats.last_fps.ToString("0.0");
    }



}
