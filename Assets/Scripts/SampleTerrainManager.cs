using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrainManager : MonoBehaviour
{
    public SampleTerrain[] allSamples;

    [Header("Dev Tile references")]
    public Tile dev_entryTile;
    public Tile dev_exitHorizontal;
    public Tile dev_exitUp;
    public Tile dev_exitDown;


    public void LoadAllSampleTerrain()
    {
        allSamples = new SampleTerrain[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            SampleTerrain s = transform.GetChild(i).GetComponent<SampleTerrain>();
            allSamples[i] = s;
        }
    }
}
