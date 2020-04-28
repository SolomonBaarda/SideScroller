using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CameraPath : MonoBehaviour
{
    private VertexPath path;
    private BezierPath b;
    public Vector2[] points;
    public Vector3[] actualPoints;

    public Vector2Int nextChunk;

    public const float autoControlLength = 0.2f;

    public void SetPath(Vector2[] points, Vector2Int nextChunk)
    {
        this.points = points;
        this.nextChunk = nextChunk;

        // Create a new path with those points
        b = new BezierPath(points, false, PathSpace.xy);

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

    public float GetLengthAtPoint(Vector2 point)
    {
        return path.GetClosestDistanceAlongPath(GetClosestPosition(point));
    }

    public Vector2 GetPositionAtDistance(float distance)
    {
        return path.GetPointAtDistance(distance);
    }

    public Vector2 GetClosestPosition(Vector2 position)
    {
        return path.GetClosestPointOnPath(position);
    }


    private void OnDrawGizmos()
    {
        foreach (Vector2 point in points)
        {
            // Draw the paths
            for (int w = 1; w < path.localPoints.Length; w++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(path.localPoints[w - 1], path.localPoints[w]);
            }

            // Draw all the points
            Gizmos.color = Color.green;
            Gizmos.DrawCube(point, 0.5f * Vector2.one);
        }

    }

}
