using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;
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
    public GameObject preset_menu_item_frame;
    public GameObject preset_item_prefab;

    [Space]
    public Button settings_menu_button;
    public GameObject settings_menu;

    [Header("Slider")]
    public GameObject map_length_slider_parent;
    public Slider map_length_slider;

    private readonly List<string> defaultPresetValues = new List<string>
    {
        Presets.Value.Default.ToString(),
        Presets.Value.Less.ToString(),
        Presets.Value.More.ToString(),
        Presets.Value.Random.ToString(),
    };

    private Presets presets;
    private Dictionary<Presets.Conversion, PresetItem> itemReferenceToVariable = new Dictionary<Presets.Conversion, PresetItem>();


    private void Awake()
    {
        // Add all the button methods
        play_button.onClick.AddListener(OnPlayPressed);
        preset_menu_button.onClick.AddListener(OnShowPresetMenu);
        settings_menu_button.onClick.AddListener(OnShowSettingsMenu);

        // Set the close buttons
        foreach (Button b in all_close_menu_buttons)
        {
            b.onClick.AddListener(OnCloseAllMenus);
        }

        OnCloseAllMenus();


        // Set up the preset window
        LoadPresetMenu();
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

        presets.DoSinglePlayer = !multiplayer_toggle.isOn;

        // Check each entry and assign its value to the corresponding variable
        foreach (Presets.Conversion key in itemReferenceToVariable.Keys)
        {
            itemReferenceToVariable.TryGetValue(key, out PresetItem i);

            // Parse the enum type
            string desciption = i.dropdown.options[i.dropdown.value].text;
            Presets.Value value = (Presets.Value)Enum.Parse(typeof(Presets.Value), desciption);

            switch (key)
            {
                case Presets.Conversion.Player_Gravity:
                    presets.player_gravity = value;
                    break;
                case Presets.Conversion.Player_Speed:
                    break;
                default:
                    Debug.LogError("Preset conversion enum undefined.");
                    break;
            }
        }

        // Load the game
        SceneLoader.Instance.LoadGame(presets);
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
        for (int i = 0; i < allMenusParent.transform.childCount; i++)
        {
            allMenusParent.transform.GetChild(i).gameObject.SetActive(false);
        }
        allMenusParent.SetActive(true);
    }


    private void LoadPresetMenu()
    {
        Transform t = preset_menu_item_frame.transform;
        presets = new Presets();

        // Player gravity
        string playerGrav = Presets.Conversion.Player_Gravity.ToString();
        itemReferenceToVariable.Add(Presets.Conversion.Player_Gravity, AddPresetItem(t, defaultPresetValues, playerGrav));
    }



    private PresetItem AddPresetItem(Transform parent, List<string> dropdownValues, string description)
    {
        GameObject newPresetItem = Instantiate(preset_item_prefab, parent);
        PresetItem i = newPresetItem.GetComponent<PresetItem>();

        i.SetDropdownValues(dropdownValues);
        i.SetDescriptionText(description);

        return i;
    }

}
