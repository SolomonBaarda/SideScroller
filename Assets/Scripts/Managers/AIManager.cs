using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Collections;

public class AIManager : MonoBehaviour
{
    public const int UpdateDistanceTiles = 8;

    public GameObject graphParent;
    public ProceduralGridMover updater;



    private void Start()
    {
        // Disable all graphs by default
        graphParent.SetActive(false);
        updater.enabled = false;
    }


    public void SetMeshUpdate(Transform toFollow)
    {
        graphParent.SetActive(true);
        AstarPath.active.Scan();

        updater.target = toFollow;

        // Enable the graphs
        updater.enabled = true;

    }


}
