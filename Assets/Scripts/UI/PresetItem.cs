using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PresetItem : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Toggle toggle;
    public TMP_Text description;

    public Presets.Type type;

    public void SetDescriptionText(string text)
    {
        description.text = text;
    }



    public object GetValue()
    {
        switch (type)
        {
            case Presets.Type.Boolean:
                return GetToggleOption();
            case Presets.Type.Value:
                return GetDropdownOption();
        }

        return null;
    }


    public string GetDropdownOption()
    {
        return dropdown.options[dropdown.value].text;
    }

    public bool GetToggleOption()
    {
        return toggle.isOn;
    }


    public void SetDropdownValues(List<string> names)
    {
        // Set the options 
        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();
        foreach (string s in names)
        {
            optionData.Add(new TMP_Dropdown.OptionData(s));
        }
        dropdown.options = optionData;
    }

    public void SetToggleValue(bool value)
    {
        toggle.isOn = value;
    }
}
