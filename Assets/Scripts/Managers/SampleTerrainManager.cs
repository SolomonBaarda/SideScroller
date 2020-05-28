using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrainManager : MonoBehaviour
{
    // Arrays for storing the Sample Terrain
    [HideInInspector]
    public SampleTerrain[] allSamples;
    [HideInInspector]
    public SampleTerrain startingArea;
    [HideInInspector]
    public SampleTerrain finishArea;

    // Dev tile types used 
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
    [Space(8)]
    public Tile dev_spawnRight;
    public Tile dev_spawnLeft;
    public Tile dev_spawnBoth;
    [Space(8)]
    public Tile dev_finish;
    [Space(8)]
    public Tile dev_cameraPathRight;
    public Tile dev_cameraPathLeft;
    public Tile dev_cameraPathUp;
    public Tile dev_cameraPathDown;
    [Space(8)]
    public Tile dev_itemCoin;
    public Tile dev_itemPot;
    public Tile dev_itemChest;

    public List<(TileBase, TileBase)> tilesToSwapWhenInverted;
    [Header("Tiles to be swapped when inverted")]
    public RuleTile rampLeft;
    public RuleTile rampRight;
    public RuleTile groundLeft;
    public RuleTile groundRight;
    public RuleTile roofLeft;
    public RuleTile roofRight;
    public WeightedRandomTile spikeLeft;
    public WeightedRandomTile spikeRight;



    public void LoadAllSampleTerrain()
    {
        // Load terrain
        Transform terrain = transform.Find("Terrain");
        allSamples = terrain.GetComponentsInChildren<SampleTerrain>();
        for (int i = 0; i < allSamples.Length; i++)
        {
            allSamples[i].LoadSample(i);
        }
        terrain.gameObject.SetActive(false);

        // Load spawn
        Transform spawn = transform.Find("Spawn");
        startingArea = spawn.GetComponentInChildren<SampleTerrain>();
        startingArea.LoadSample(0);
        spawn.gameObject.SetActive(false);

        // Load spawn
        Transform finish = transform.Find("Finish");
        finishArea = finish.GetComponentInChildren<SampleTerrain>();
        finishArea.LoadSample(0);
        finish.gameObject.SetActive(false);

        // Add all the tiles to the list
        tilesToSwapWhenInverted = new List<(TileBase, TileBase)>
        {
            (rampLeft, rampRight),
            (groundLeft, groundRight),
            (roofLeft, roofRight),
            (spikeLeft, spikeRight)
        };

    }
}
