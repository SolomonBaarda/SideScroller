using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardRow : MonoBehaviour
{
    public GameObject row;

    public GameObject[] columns;


    public void SetColumnText(GameObject column, string text)
    {
        TMP_Text t = column.GetComponent<TMP_Text>();
        if (t != null)
        {
            t.text = text;
        }
    }
}
