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

    private void Awake()
    {
        // Set the button text
        SetMenuTextColour(play_button);

        // Add the event calls and functions
        OnButtonClicked += Unload;
        play_button.GetComponent<Button>().onClick.AddListener(OnButtonClicked);

        // Disable the button by default
        play_button.enabled = false;

        // Enable the button when the game is ready
        TerrainManager.OnTerrainGenerated += Ready;
    }


    private void Start()
    {
        // Load the main game
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
    }


    private void OnDestroy()
    {
        // Remove listeners
        OnButtonClicked -= Unload;
        TerrainManager.OnTerrainGenerated -= Ready;

        play_button.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    private void Ready()
    {
        Debug.Log("terrain ready!");
        play_button.enabled = true;
    }


    private void Unload()
    {
        OnMenuClose.Invoke();
        SceneManager.UnloadSceneAsync(0);
    }

    private void SetMenuTextColour(TMP_Text t)
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
