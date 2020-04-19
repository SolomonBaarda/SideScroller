using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrainManager : MonoBehaviour
{
    public SampleTerrain[] allSamples;

    [HideInInspector]
    public SampleTerrain startingArea;

    [Header("Dev Tile references")]
    public Tile dev_entryRight;
    public Tile dev_entryLeft;
    public Tile dev_entryUp;
    public Tile dev_entryDown;
    [Space(8)]
    public Tile dev_exitRight;
    public Tile dev_exitLeft;
    public Tile dev_exitUp;
    public Tile dev_exitDown;


    public void LoadAllSampleTerrain()
    {
        // Load terrain
        Transform terrain = transform.Find("Terrain");
        allSamples = terrain.GetComponentsInChildren<SampleTerrain>();
        foreach (SampleTerrain t in allSamples)
        {
            t.LoadSample();
        }


        // Load spawn
        Transform spawn = transform.Find("Spawn");
        startingArea = spawn.GetComponentInChildren<SampleTerrain>();
        startingArea.LoadSample();
    }
}
