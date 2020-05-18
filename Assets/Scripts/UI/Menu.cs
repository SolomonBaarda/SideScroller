﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Menu : MonoBehaviour
{
    public static UnityAction OnMenuClose;

    [Header("Play Button")]
    public TMP_Text play_button;

    [Header("Multiplayer Toggle")]
    public Toggle multiplayer_toggle;

    [Header("Slider")]
    public GameObject map_length_slider_parent;
    public Slider map_length_slider;

    private void Awake()
    {
        // Set the button text
        SetMenuStyleTMPro(ref play_button);

        LoadPresetMenu();
    }


    private void Update()
    {
        // Only enable the slider if it is multiplayer mode
        map_length_slider_parent.SetActive(multiplayer_toggle.isOn);
    }


    public void OnPlayPressed()
    {
        // Disable the button
        play_button.enabled = false;

        // Apply the presets
        GameManager.Presets preset = new GameManager.Presets
        {
            DoSinglePlayer = !multiplayer_toggle.isOn,
            terrain_limit_not_endless = (int)map_length_slider.value,
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

        //AddDropdown(presetMenuParent, "Player speed", allPresetValues);
    }



    private void AddDropdown(Transform parent, string item_text, List<GameManager.PresetValues> presetOptions)
    {
        GameObject newDropdown = new GameObject(item_text);
        newDropdown.transform.parent = parent;
        TMP_Dropdown d = newDropdown.AddComponent<TMP_Dropdown>();

        GameObject text = new GameObject("text");
        text.transform.parent = newDropdown.transform;
        TMP_Text t = text.AddComponent<TextMeshProUGUI>();
        SetMenuStyleTMPro(ref t);
        t.text = item_text;

        d.itemText = t;

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (GameManager.PresetValues p in presetOptions)
        {
            options.Add(new TMP_Dropdown.OptionData
            {
                text = p.ToString()
            });
        }

        d.options = options;

        d.template = parent.GetComponent<RectTransform>();
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