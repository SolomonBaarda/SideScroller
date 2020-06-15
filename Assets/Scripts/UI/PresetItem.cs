using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PresetItem : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public TMP_Text description;



    public void SetDescriptionText(string text)
    {
        description.text = text;
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
}
