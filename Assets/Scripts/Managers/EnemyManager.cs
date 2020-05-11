using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyManager : MonoBehaviour
{
    public Transform player;

    private GridGraph graph;

    private const int DEFAULT_GRAPH_INCREMENT = 124;

    private void Start()
    {
        graph = AstarPath.active.data.gridGraph;
    }



    public void ScanWholeNavMesh()
    {
        // Update all paths
        AstarPath.active.Scan();
    }


    public void CheckUpdateSize(Vector2Int absDistanceFromOrigin)
    {
        Vector2Int currentGroundExtents = new Vector2Int(graph.width, graph.depth);
        bool needToRecalculate = false;

        // Change to bounds
        absDistanceFromOrigin *= 2;

        while (absDistanceFromOrigin.x >= currentGroundExtents.x)
        {
            currentGroundExtents.x += DEFAULT_GRAPH_INCREMENT;
            needToRecalculate = true;
        }
        while (absDistanceFromOrigin.y >= currentGroundExtents.y)
        {
            currentGroundExtents.y += DEFAULT_GRAPH_INCREMENT;
            needToRecalculate = true;
        }


        if (needToRecalculate)
        {
            UpdateNavMeshSize(currentGroundExtents);
        }
    }


    private void UpdateNavMeshSize(Vector2Int graphSizeTiles)
    {
        // Update the new graph size
        graph.SetDimensions(graphSizeTiles.x, graphSizeTiles.y, 1.0f);

        ScanWholeNavMesh();
    }


    public void UpdateNavMesh(Bounds bounds)
    {
        // Update all paths
        AstarPath.active.UpdateGraphs(bounds);
    }
}
