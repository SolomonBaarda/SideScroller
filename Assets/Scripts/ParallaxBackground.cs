using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxBackground : MonoBehaviour
{
    private float spriteLength, startPosition;
    public GameObject cam;
    public float parallaxStrength;


    private void Awake()
    {
        startPosition = transform.position.x;
        spriteLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }




    private void Update()
    {
        float offsetRelativeToCam = cam.transform.position.x * (1 - parallaxStrength);
        float offset = cam.transform.position.x * parallaxStrength;

        transform.position = new Vector2(startPosition + offset, transform.position.y);

        if(offsetRelativeToCam > startPosition + spriteLength)
        {
            startPosition += spriteLength;
        }
        else if (offsetRelativeToCam < startPosition - spriteLength)
        {
            startPosition -= spriteLength;
        }
    }
}
