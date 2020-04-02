using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public GameObject following;
    public float zoom = 10;

    private void Start()
    {
        Vector3 pos = transform.position;
        pos.z = -zoom;
        transform.position = pos;
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 pos = following.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;

        GetComponent<Camera>().orthographicSize = zoom;
    }
}
