using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CameraPath : MonoBehaviour
{
    private VertexPath path;
    public List<Vector3> points;
    public Vector3[] actualPoints;

    public const float autoControlLength = 0.4f;

    public void SetPath(Vector3 start, Vector3 end)
    {
        points = new List<Vector3>();
        points.Add(start);
        points.Add(end);

        // Create a new path with those points
        BezierPath b = new BezierPath(points, false, PathSpace.xy);

        // Set the correct modes
        b.ControlPointMode = BezierPath.ControlMode.Automatic;
        b.AutoControlLength = autoControlLength;

        // Reference to the path
        path = new VertexPath(b, transform.root);

        actualPoints = path.localPoints;
    }



    public float GetPathLength()
    {
        return path.length;
    }

    public float GetLengthAtPoint(Vector3 point)
    {
        return path.GetClosestDistanceAlongPath(GetClosestPosition(point));
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        return path.GetPointAtDistance(distance);
    }

    public Vector3 GetClosestPosition(Vector3 position)
    {
        return path.GetClosestPointOnPath(position);
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 point in points)
        {
            // Draw the paths
            for (int w = 1; w < path.localPoints.Length; w++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(path.localPoints[w - 1], path.localPoints[w]);
            }

            // Draw all the points
            Gizmos.color = Color.green;
            Gizmos.DrawCube(point, 0.5f * Vector3.one);
        }

    }

}
