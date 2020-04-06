using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrainManager : MonoBehaviour
{
    public List<SampleTerrain> allSamples;

    [Header("Dev Tile references")]
    public Tile dev_entryTile;
    public Tile dev_exitTile;

    // Start is called before the first frame update
    void Start()
    {
        allSamples = new List<SampleTerrain>();
    }

    public void LoadAllSampleTerrain()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            SampleTerrain s = transform.GetChild(i).GetComponent<SampleTerrain>();
            allSamples.Add(s);
        }
    }
}
