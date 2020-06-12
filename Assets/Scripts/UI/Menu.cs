using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEditor.Build.Player;

public class Menu : MonoBehaviour
{
    public static UnityAction OnMenuClose;

    [Header("Play Button")]
    public Button play_button;

    [Header("Multiplayer Toggle")]
    public Toggle multiplayer_toggle;

    [Header("Slider")]
    public GameObject map_length_slider_parent;
    public Slider map_length_slider;

    private void Awake()
    {
        LoadPresetMenu();

        play_button.onClick.AddListener(OnPlayPressed);
    }

    private void OnDestroy()
    {
        play_button.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        // Only enable the slider if it is multiplayer mode
        //map_length_slider_parent.SetActive(multiplayer_toggle.isOn);
    }


    public void OnPlayPressed()
    {
        // Disable the button
        play_button.enabled = false;

        // Apply the presets
        GameManager.Presets preset = new GameManager.Presets
        {
            DoSinglePlayer = !multiplayer_toggle.isOn,
            //terrain_limit_not_endless = (int)map_length_slider.value,
        };

        // Load the game
        SceneLoader.Instance.LoadGame(preset);
    }


    private void LoadPresetMenu()
    {
        List<GameManager.PresetValues> allPresetValues = new List<GameManager.PresetValues>();
        allPresetValues.Add(GameManager.PresetValues.Default);
        allPresetValues.Add(GameManager.PresetValues.Less);
        allPresetValues.Add(GameManager.PresetValues.More);
        allPresetValues.Add(GameManager.PresetValues.Random);
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
