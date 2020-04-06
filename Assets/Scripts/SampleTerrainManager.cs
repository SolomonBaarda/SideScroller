using System.Collections.Generic;
using UnityEngine;

public class SampleTerrainManager : MonoBehaviour
{
    public List<SampleTerrain> allSamples;

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
