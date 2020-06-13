using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Menu : MonoBehaviour
{
    public static UnityAction OnMenuClose;

    [Header("Play Button")]
    public Button play_button;

    [Header("Multiplayer Toggle")]
    public Toggle multiplayer_toggle;

    [Header("Menus")]
    public Button showPresetMenu;
    public GameObject presetMenu;
    public Button showSettingsMenu;
    public GameObject settingsMenu;

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
        List<GameManager.PresetValues> allPresetValues = new List<GameManager.PresetValues>
        {
            GameManager.PresetValues.Default,
            GameManager.PresetValues.Less,
            GameManager.PresetValues.More,
            GameManager.PresetValues.Random
        };
    }

}
