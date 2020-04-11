using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CameraPath : MonoBehaviour
{
    private PathCreator pathCreator;
    private List<Vector3> points;

    void Awake()
    {
        pathCreator = GetComponent<PathCreator>();
        points = new List<Vector3>();
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
