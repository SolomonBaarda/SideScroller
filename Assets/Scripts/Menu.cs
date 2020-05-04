using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;


public class Menu : MonoBehaviour
{
    public TMP_Text play_button;
    private static UnityAction OnButtonClicked;
    public static UnityAction OnMenuClose;

    private GameManager.Presets preset;

    private void Awake()
    {
        // Set the button text
        SetMenuStyleTMPro(ref play_button);

        // Disable the button by default
        play_button.enabled = false;

        // Add the event calls and functions
        //play_button.GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        //OnButtonClicked += ApplyGamePresets;

        TerrainManager.OnInitialTerrainGenerated += Unload;

        preset = new GameManager.Presets();

        Ready();
        LoadGame();
    }


    private void LoadGame()
    {
        // Load the main game
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
    }


    private void OnDestroy()
    {
        // Remove listeners
        OnButtonClicked -= ApplyGamePresets;
        TerrainManager.OnInitialTerrainGenerated -= Ready;

        play_button.GetComponent<Button>().onClick.RemoveAllListeners();
    }


    private void Ready()
    {
        play_button.enabled = true;
    }


    public void ApplyGamePresets()
    {
        // Apply the preset
        GameManager.OnSetPresets.Invoke(preset);

        // Disable the play button
        play_button.enabled = false;
    }


    private void Unload()
    {
        OnMenuClose.Invoke();
        SceneManager.UnloadSceneAsync(0);
    }

    public static void SetMenuStyleTMPro(ref TMP_Text t)
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
