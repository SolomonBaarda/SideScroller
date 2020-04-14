using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrainManager : MonoBehaviour
{
    public SampleTerrain[] allSamples;

    [HideInInspector]
    public SampleTerrain startingArea;

    [Header("Dev Tile references")]
    public Tile dev_entryTile;
    public Tile dev_exitHorizontal;
    public Tile dev_exitUp;
    public Tile dev_exitDown;


    public void LoadAllSampleTerrain()
    {
        // Load terrain
        Transform terrain = transform.Find("Terrain");
        allSamples = terrain.GetComponentsInChildren<SampleTerrain>();


        // Load spawn
        Transform spawn = transform.Find("Spawn");
        startingArea = spawn.GetComponentInChildren<SampleTerrain>();
    }
}
