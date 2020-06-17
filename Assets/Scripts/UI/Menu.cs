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
    public GameObject preset_variable_item_prefab;
    public GameObject preset_bool_item_prefab;

    [Space]
    public Button settings_menu_button;
    public GameObject settings_menu;

    [Header("Slider")]
    public GameObject map_length_slider_parent;
    public Slider map_length_slider;



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


    public void OnPlayPressed()
    {
        // Disable the button
        play_button.enabled = false;

        // Check each entry and assign its value to the corresponding variable
        foreach (Presets.Conversion key in itemReferenceToVariable.Keys)
        {
            itemReferenceToVariable.TryGetValue(key, out PresetItem i);

            // Get the preset value
            object value = i.GetValue();

            // Parse if it is the enum type
            if (i.type == Presets.Type.Value)
            {
                value = (Presets.Value)Enum.Parse(typeof(Presets.Value), i.GetDropdownOption());
            }

            // Set the presets
            presets.SetPreset(key, value);

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

        // Game rule settings
        itemReferenceToVariable.Add(Presets.Conversion.Multiplayer, AddTogglePreset(t, presets.DoMultiplayer, "Multiplayer"));
        itemReferenceToVariable.Add(Presets.Conversion.Random_Weapons, AddTogglePreset(t, presets.DoSpawnWithRandomWeapons, "Random Weapons"));
        itemReferenceToVariable.Add(Presets.Conversion.Item_Drops, AddTogglePreset(t, presets.DoItemDrops, "Item Drops"));
        itemReferenceToVariable.Add(Presets.Conversion.Item_Spawns, AddTogglePreset(t, presets.DoGenerateItemsWithWorld, "Spawn Items"));

        // Generation settings
        itemReferenceToVariable.Add(Presets.Conversion.Map_Length, AddDropdownPreset(t, Presets.DefaultValueStrings, "Map Length"));

        // Player settings
        itemReferenceToVariable.Add(Presets.Conversion.Gravity_Modifier, AddDropdownPreset(t, Presets.DefaultValueStrings, "Gravity Scale"));
        itemReferenceToVariable.Add(Presets.Conversion.Player_Speed, AddDropdownPreset(t, Presets.DefaultValueStrings, "Player Speed"));
    }



    private PresetItem AddDropdownPreset(Transform parent, List<string> dropdownValues, string description)
    {
        GameObject newPresetItem = Instantiate(preset_variable_item_prefab, parent);
        PresetItem i = newPresetItem.GetComponent<PresetItem>();

        i.type = Presets.Type.Value;
        i.SetDropdownValues(dropdownValues);
        i.SetDescriptionText(description);

        return i;
    }

    private PresetItem AddTogglePreset(Transform parent, bool isOn, string description)
    {
        GameObject newPresetItem = Instantiate(preset_bool_item_prefab, parent);
        PresetItem i = newPresetItem.GetComponent<PresetItem>();

        i.type = Presets.Type.Boolean;
        i.SetToggleValue(isOn);
        i.SetDescriptionText(description);

        return i;
    }

}
