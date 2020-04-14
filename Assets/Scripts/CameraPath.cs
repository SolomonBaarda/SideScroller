using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CameraPath : MonoBehaviour
{
    private PathCreator pathCreator;
    private List<Vector3> points;

    public float autoControlLength = 0.4f;

    private void Awake()
    {
        pathCreator = GetComponent<PathCreator>();
        points = new List<Vector3>();

        // Set the mode
        pathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
        pathCreator.bezierPath.AutoControlLength = autoControlLength;
        pathCreator.bezierPath.Space = PathSpace.xy;

        // Move points to 00
        for (int i = 0; i < pathCreator.bezierPath.NumPoints; i++)
        {
            pathCreator.bezierPath.MovePoint(i, Vector3.zero);
        }
    }


    public float GetPathLength()
    {
        return pathCreator.path.length;
    }


    public void AddPoint(Vector3 point)
    {
        points.Add(point);
        pathCreator.bezierPath.AddSegmentToEnd(point);
        //pathCreator.TriggerPathUpdate();
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        return pathCreator.path.GetPointAtDistance(Mathf.Abs(distance));
    }

    public Vector3 GetClosestPosition(Vector3 position)
    {
        return pathCreator.path.GetClosestPointOnPath(position);
    }




}
