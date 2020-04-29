using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 40;
    [SerializeField] private float jump_force = 12;
    [SerializeField] private int default_double_jumps = 1;
    [SerializeField] private int max_double_jumps = 0;
    [SerializeField] private int double_jumps_left = 0;
    [Range(0, 1)] [SerializeField] private float crouch_speed_reduction = .4f;

    [Header("Controller Settings")]
    [Range(0, .5f)] [SerializeField] private float movementSmoothing = .02f;
    [SerializeField] private bool allowAirControl = true;
    private const float collisionCheckRadius = .2f;

    public readonly Vector2 TERMINAL_VELOCITY = new Vector2(16, 124);
    private Vector2 previousVelocity = Vector2.zero;
    private bool previouslyGrounded = false;

    [SerializeField] private Direction facing;

    private Rigidbody2D rigid;
    private Collider2D mainCollider;
    private Collider2D feetCollider;
    public Transform headPos;
    public Transform feetPos;

    private enum Direction { Left, Right };


    private void Awake()
    {
        mainCollider = GetComponent<BoxCollider2D>();
        feetCollider = GetComponent<CircleCollider2D>();
        rigid = GetComponent<Rigidbody2D>();

        max_double_jumps = default_double_jumps;
    }

    private void OnEnable()
    {
        // Face forward by default
        facing = Direction.Right;
    }


    private void OnDisable()
    {
        rigid.velocity = Vector2.zero;
    }


    private void FixedUpdate()
    {
        // Check if the player is on the ground
        previouslyGrounded = IsOnGround;
        IsOnGround = false;

        // Get all collisions
        Collider2D[] collisions = Physics2D.OverlapCircleAll(feetPos.position, collisionCheckRadius, LayerMask.GetMask("Ground"));
        for (int i = 0; i < collisions.Length; i++)
        {
            if (collisions[i].gameObject != gameObject)
            {
                IsOnGround = true;
                double_jumps_left = max_double_jumps;
            }
        }
    }


    public void Move(float move, bool crouch, bool jump)
    {
        move *= speed;

        // If crouch has been let go of
        if (!crouch)
        {
            if (IsCrouching)
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(headPos.position, collisionCheckRadius, LayerMask.GetMask("Ground")))
                {
                    crouch = true;
                }
            }
        }


        // Allow user direction movement input 
        if (IsOnGround || allowAirControl)
        {
            // Set crouching
            if (crouch)
            {
                // Player has just crouched this frame
                if (!IsCrouching)
                {
                    IsCrouching = true;
                }

                // Disable the top collider when crouching
                if (mainCollider != null)
                {
                    mainCollider.enabled = false;
                }
            }
            // Set not crouching
            else
            {
                // Enable the collider when not crouching
                if (mainCollider != null)
                {
                    mainCollider.enabled = true;
                }

                // Player has just uncrouched
                if (IsCrouching)
                {
                    IsCrouching = false;
                }
            }

            // Movement stuff

            // Desired velocity
            Vector2 targetVelocity = new Vector2(move * 10, rigid.velocity.y);

            bool isSliding = false;

            // Player is crouching, overwrite target vel
            if (IsCrouching && IsOnGround)
            {
                // Player has just landed from falling
                if(!previouslyGrounded)
                {
                    // Do a big slide in the direction the player is facing

                    Vector2 newVelocity = rigid.velocity;

                    int dir = 0;
                    if (facing.Equals(Direction.Right))
                    {
                        dir = 1;
                    }
                    else if (facing.Equals(Direction.Left))
                    {
                        dir = -1;
                    }

                    newVelocity.x = dir * Mathf.Abs(previousVelocity.y);
                    rigid.velocity = newVelocity;
                }

                // Reduce the speed by the crouch speed reduction
                targetVelocity.x = Mathf.MoveTowards(rigid.velocity.x, 0, crouch_speed_reduction);
                isSliding = true;
            }


            // Get the new velocity and ensure it doesn't exceed terminal velocity
            Vector3 newVel = Vector2.SmoothDamp(rigid.velocity, targetVelocity, ref previousVelocity, movementSmoothing);
            newVel.x = Mathf.Clamp(newVel.x, -TERMINAL_VELOCITY.x, TERMINAL_VELOCITY.x);
            newVel.y = Mathf.Clamp(newVel.y, -TERMINAL_VELOCITY.y, TERMINAL_VELOCITY.y);

            // Assign the new velocity
            previousVelocity = rigid.velocity = newVel;


            // Can't change direction when sliding
            if (!isSliding)
            {
                // If the input is moving the player right and the player is facing left
                if (move > 0 && !facing.Equals(Direction.Right))
                {
                    // Face right
                    Face(Direction.Right);
                }
                // Otherwise if the input is moving the player left and the player is facing right
                else if (move < 0 && !facing.Equals(Direction.Left))
                {
                    // Face left
                    Face(Direction.Left);
                }
            }

        }

        // Regular jump
        if (IsOnGround && jump)
        {
            // Add a vertical force to the player.
            IsOnGround = false;
            Jump(jump_force);
        }
        // Double jump
        else if (!IsOnGround && jump && double_jumps_left > 0)
        {
            double_jumps_left--;
            Jump(jump_force);
        }

    }



    private void Jump(float force)
    {
        // Set y vel to 0
        Vector2 vel = rigid.velocity;
        vel.y = 0;
        rigid.velocity = vel;

        // Then jump
        rigid.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
    }


    private void ForwardJump(float forceX, float forceY)
    {
        // Set y vel to 0
        rigid.velocity = Vector2.zero;

        // Then jump
        rigid.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
    }


    private void Face(Direction d)
    {
        facing = d;

        // Flip the player
        Vector3 scale = transform.localScale;

        int i = 0;
        if (d == Direction.Left)
        {
            i = -1;
        }
        else if (d == Direction.Right)
        {
            i = 1;
        }

        scale.x = i;
        transform.localScale = scale;
    }


    private void Flip()
    {
        // Flip the player
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }


    public Vector2 PlayerVelocity { get { return rigid.velocity; } }

    public bool IsCrouching { get; private set; }
    public bool IsOnGround { get; private set; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(headPos.position, collisionCheckRadius);
        Gizmos.DrawSphere(feetPos.position, collisionCheckRadius);
    }



}
