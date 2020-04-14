using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathManager : MonoBehaviour
{
    private CameraPath leftPath, rightPath;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);

            if (t.name.Contains("Left"))
            {
                leftPath = t.GetComponent<CameraPath>();
            }
            else if (t.name.Contains("Right"))
            {
                rightPath = t.GetComponent<CameraPath>();
            }
        }
    }


    public void AddPointLeft(Vector3 point)
    {
        leftPath.AddPoint(point);
    }

    public void AddPointRight(Vector3 point)
    {
        rightPath.AddPoint(point);
    }


    public Vector2 GetPathLength()
    {
        return new Vector2(leftPath.GetPathLength(), rightPath.GetPathLength());
    }


    public Vector3 GetClosestPoint(Vector3 position)
    {
        Vector3 l = leftPath.GetClosestPosition(position);
        Vector3 r = rightPath.GetClosestPosition(position);

        return l;

        // Return the closest point
        if (Vector3.Distance(position, l) < Vector3.Distance(position, r))
        {
            return l;
        }
        else
        {
            return r;
        }
    }


    public Vector3 GetPointAtDistance(float distance)
    {
        Vector3 position = Vector3.zero;

        // Still at origin
        if (distance == 0)
        {
            Debug.LogError("Camera path at 000");
        }
        // Move right
        else if (distance > 0)
        {
            position = rightPath.GetPositionAtDistance(distance);
        }
        // Move left
        else if (distance < 0)
        {
            position = leftPath.GetPositionAtDistance(Mathf.Abs(distance));
        }

        return position;
    }



}
