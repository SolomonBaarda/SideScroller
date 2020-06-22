using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Follower : MonoBehaviour
{
    public const float UPDATE_PATH_DEFAULT_FREQUENCY = 0.25f;
    public const float nextWaypointDistance = 3f;

    public Transform Target;
    public float Speed = 400;

    public Path Path { get; private set; }
    private int currentWaypoint;
    public bool ReachedEndOfPath { get; private set; } = false;
    public bool IsFollowing { get; private set; }

    private Seeker seeker;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;



    private void Awake()
    {
        seeker = gameObject.AddComponent<Seeker>();
        SimpleSmoothModifier s = gameObject.AddComponent<SimpleSmoothModifier>();
        s.seeker = seeker;
        s.smoothType = SimpleSmoothModifier.SmoothType.CurvedNonuniform;

        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        InvokeRepeating("CalculateNewPath", 1, UPDATE_PATH_DEFAULT_FREQUENCY);
    }



    private void CalculateNewPath()
    {
        if (seeker.IsDone() && Target != null)
        {
            // Calculate new path
            seeker.StartPath(rigid.position, Target.position, OnPathCalculated);
        }
    }


    private void OnPathCalculated(Path path)
    {
        if (!path.error)
        {
            // Update the path
            Path = path;
            currentWaypoint = 0;
        }
    }



    private void FixedUpdate()
    {
        if (IsFollowing)
        {
            if (Path != null)
            {
                // Update reachedEndOfPath
                if (currentWaypoint >= Path.vectorPath.Count)
                {
                    ReachedEndOfPath = true;
                    return;
                }
                else
                {
                    ReachedEndOfPath = false;
                }

                // Update the current waypoint and distance
                float distanceToWaypoint = Vector2.Distance(rigid.position, Path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance)
                {
                    currentWaypoint++;
                }

                // Value between 0 and 1 determining how fast we should move
                float speedMultiplier = ReachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

                // Direction vector to the current waypoint
                Vector2 direction = ((Vector2)Path.vectorPath[currentWaypoint] - rigid.position).normalized;
                Vector2 velocity = direction * Speed * speedMultiplier * Time.fixedDeltaTime;

                // Apply movement force to enemy
                rigid.AddForce(velocity);


                // Update which way the sprite is facing
                bool flip = false;
                if (direction.x < 0)
                {
                    flip = true;
                }
                spriteRenderer.flipX = flip;
            }
        }

    }


    public void SetFollowing(bool follow, float initialGravityScale)
    {
        IsFollowing = follow;

        if (follow)
        {
            rigid.gravityScale = 0;
            rigid.drag = 1;
            rigid.freezeRotation = true;
        }
        else
        {
            rigid.gravityScale = initialGravityScale;
            rigid.drag = 0;
            rigid.freezeRotation = false;
            Target = null;
        }
    }




}
