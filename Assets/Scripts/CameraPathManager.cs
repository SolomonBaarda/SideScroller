using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathManager : MonoBehaviour
{
    public static Vector3 GetClosestPoint(Vector3 position, Chunk chunk)
    {
        CameraPath[] paths = chunk.cameraPaths.ToArray();
        Vector3 point = paths[0].GetClosestPosition(position);

        // Loop through each path
        foreach (CameraPath path in paths)
        {
            float posToPoint = Vector3.Distance(position, point);
            Vector3 newPoint = path.GetClosestPosition(position);
            float posToNewPoint = Vector3.Distance(position, newPoint);

            // Find the closest point of all the paths
            if (posToNewPoint < posToPoint)
            {
                point = newPoint;
            }
        }

        return point;
    }


}
