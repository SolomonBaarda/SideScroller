using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item : ScriptableObject
{
    // Sprite to display
    public Sprite sprite;

    // Name of the item in inventory
    public string display_name;
}
