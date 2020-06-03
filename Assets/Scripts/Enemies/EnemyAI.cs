using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 200f;
    public float nextWaypointDistance = 3f;

    public Transform target;

    private Path path;
    private int currentWaypoint;
    bool reachedEndOfPath = false;

    private Seeker seeker;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        InvokeRepeating("CalculateNewPath", 0f, 0.5f);
    }


    private void CalculateNewPath()
    {
        if(seeker.IsDone())
        {
            // Calculate new path
            seeker.StartPath(rigid.position, target.position, OnPathCalculated);
        }
    }

    private void OnPathCalculated(Path path)
    {
        if(!path.error)
        {
            // Set the path
            this.path = path;
            currentWaypoint = 0;
        }
    }


    private void Update()
    {
        if(reachedEndOfPath)
        {

        }
    }


    private void FixedUpdate()
    {
        if(path != null)
        {
            if(currentWaypoint >= path.vectorPath.Count)
            {
                reachedEndOfPath = true;
                return;
            }
            else
            {
                reachedEndOfPath = false;
            }

            // Direction vector to the current waypoint
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rigid.position).normalized;
            Vector2 force = direction * speed * Time.fixedDeltaTime;

            // Apply movement force to enemy
            rigid.AddForce(force);

            // Update the current waypoint
            float distance = Vector2.Distance(rigid.position, path.vectorPath[currentWaypoint]);
            if(distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }


            // Update which way the sprite is facing
            if(force.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (force.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }






}
