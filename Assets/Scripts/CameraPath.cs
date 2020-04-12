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

        pathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
        pathCreator.bezierPath.AutoControlLength = autoControlLength;

        for(int i = 0; i < pathCreator.bezierPath.NumPoints; i++)
        {
            pathCreator.bezierPath.MovePoint(i, Vector3.zero);
        }
        
    }


    private void CheckRemoveFirstTwoPoints()
    {
        
    }


    public void AddPointRight(Vector3 point)
    {
        points.Add(point);
        pathCreator.bezierPath.AddSegmentToEnd(point);
        pathCreator.TriggerPathUpdate();
    }

    public void AddPointLeft(Vector3 point)
    {
        points.Add(point);
        pathCreator.bezierPath.AddSegmentToStart(point);
        pathCreator.TriggerPathUpdate();
    }




}
