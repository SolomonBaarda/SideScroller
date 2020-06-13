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
    public GameObject allMenusParent;
    public Button[] all_close_menu_buttons;

    public Button preset_menu_button;
    public GameObject preset_menu;
    public Button settings_menu_button;
    public GameObject settings_menu;

    [Header("Slider")]
    public GameObject map_length_slider_parent;
    public Slider map_length_slider;

    private void Awake()
    {
        //LoadPresetMenu();

        // Add all the button methods
        play_button.onClick.AddListener(OnPlayPressed);
        preset_menu_button.onClick.AddListener(OnShowPresetMenu);
        settings_menu_button.onClick.AddListener(OnShowSettingsMenu);

        // Set the close buttons
        foreach(Button b in all_close_menu_buttons)
        {
            b.onClick.AddListener(OnCloseAllMenus);
        }

        OnCloseAllMenus();
    }

    private void OnDestroy()
    {
        // Remove all listeners
        play_button.onClick.RemoveAllListeners();
        preset_menu_button.onClick.RemoveAllListeners();
        settings_menu_button.onClick.RemoveAllListeners();

        foreach (Button b in all_close_menu_buttons)
        {
            b.onClick.RemoveAllListeners();
        }
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

    public void OnShowPresetMenu()
    {
        OnCloseAllMenus();
        preset_menu.SetActive(true);
    }

    public void OnShowSettingsMenu()
    {
        OnCloseAllMenus();
        settings_menu.SetActive(true);
    }

    public void OnCloseAllMenus()
    {
        // Disable all children menus
        for(int i = 0; i < allMenusParent.transform.childCount; i++)
        {
            allMenusParent.transform.GetChild(i).gameObject.SetActive(false);
        }
        allMenusParent.SetActive(true);
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
