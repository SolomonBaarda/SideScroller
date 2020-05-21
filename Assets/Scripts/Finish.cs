using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    private Bounds bounds;

    public void CreateFinish(Bounds bounds)
    {
        this.bounds = bounds;
    }


    private void OnDrawGizmos()
    {
        if(bounds != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(bounds.min, new Vector2(bounds.max.x, bounds.min.y));
            Gizmos.DrawLine(bounds.min, new Vector2(bounds.min.x, bounds.max.y));
            Gizmos.DrawLine(bounds.max, new Vector2(bounds.max.x, bounds.min.y));
            Gizmos.DrawLine(bounds.max, new Vector2(bounds.min.x, bounds.max.y));
        }
    }
}
