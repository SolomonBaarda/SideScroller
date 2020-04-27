using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pathfinding;

public class EnemyManager : MonoBehaviour
{
    public Transform player;

    private GridGraph graph;

    private void Awake()
    {
        // Find the graph
        if (graph == null)
        {
            graph = AstarPath.active.data.gridGraph;
        }
    }


    public void ScanWholeNavMesh()
    {
        // Update all paths
        AstarPath.active.Scan();
    }


    public void UpdateNavMesh(Bounds bounds, Vector2Int graphSizeTiles)
    {
        // Update the new graph size
        graph.SetDimensions(graphSizeTiles.x, graphSizeTiles.y, 1.0f);
        Debug.Log(graphSizeTiles);

        // Update all paths
        AstarPath.active.UpdateGraphs(bounds);
    }
}
