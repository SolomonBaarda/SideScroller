using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pathfinding;

public class EnemyManager : MonoBehaviour
{
    public Transform player;

    private GridGraph graph;
    private Vector2Int currentGroundExtents = Vector2Int.one;

    private bool canUpdateSize = true;

    private void Start()
    {
        graph = AstarPath.active.data.gridGraph;
    }


    private void FixedUpdate()
    {
        if (graph != null)
        {
            if (canUpdateSize)
            {
                // The graph size needs to be increased
                if (currentGroundExtents.x >= graph.width || currentGroundExtents.y >= graph.depth)
                {
                    //canUpdateSize = false;
                    //UpdateNavMeshSize(new Vector2Int(2 * graph.width, 2 * graph.depth));
                }
            }
        }
    }

    private void ResetSizeUpdater()
    {
        canUpdateSize = true;
    }


    public void ScanWholeNavMesh()
    {
        // Update all paths
        AstarPath.active.Scan();
    }


    public void CheckUpdateSize(Vector2Int absDistanceFromOrigin)
    {
        if (absDistanceFromOrigin.x > currentGroundExtents.x)
        {
            currentGroundExtents.x = absDistanceFromOrigin.x;
        }
        if (absDistanceFromOrigin.y > currentGroundExtents.y)
        {
            currentGroundExtents.y = absDistanceFromOrigin.y;
        }
    }


    private void UpdateNavMeshSize(Vector2Int graphSizeTiles)
    { 
        // Update the new graph size
        graph.SetDimensions(graphSizeTiles.x, graphSizeTiles.y, 1.0f);

        graph.Scan();
    }


    public void UpdateNavMesh(Bounds bounds)
    {
        // Update all paths
        //AstarPath.active.UpdateGraphs(bounds);
    }
}
