using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jump_force = 600f;
    [SerializeField] private int max_double_jumps = 1;
    [SerializeField] private int double_jumps_left = 0;
    [Range(0, 1)] [SerializeField] private float crouch_speed_reduction = .4f;

    [Header("Controller Settings")]
    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .02f;
    [SerializeField] private bool allowAirControl = true;
    private const float collisionCheckRadius = .1f;
    private bool isGrounded;
    private Rigidbody2D rigid;
    private Collider2D mainCollider;
    private Collider2D feetCollider;

    private Vector2 velocity = Vector2.zero;
    [SerializeField]
    private Direction facing;
    [SerializeField] private Transform headPos;
    [SerializeField] private Transform feetPos;

    [Header("Events")]
    [Space]
    public UnityEvent OnPlayerland;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnPlayerCrouch;
    private bool isCrouching = false;


    private enum Direction { Left, Right, Forward };

    private void Awake()
    {
        mainCollider = GetComponent<BoxCollider2D>();
        feetCollider = GetComponent<CircleCollider2D>();
        rigid = GetComponent<Rigidbody2D>();

        if (OnPlayerland == null)
        {
            OnPlayerland = new UnityEvent();
        }
        if (OnPlayerCrouch == null)
        {
            OnPlayerCrouch = new BoolEvent();
        }

    }

    private void OnEnable()
    {
        // Face forward by default
        facing = Direction.Forward;
    }


    private void OnDisable()
    {
        rigid.velocity = Vector2.zero;
    }


    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // Get all collisions
        Collider2D[] collisions = Physics2D.OverlapCircleAll(feetPos.position, collisionCheckRadius, LayerMask.GetMask("Ground"));
        for (int i = 0; i < collisions.Length; i++)
        {
            if (collisions[i].gameObject != gameObject)
            {
                isGrounded = true;
                double_jumps_left = max_double_jumps;
                if (!wasGrounded)
                {
                    OnPlayerland.Invoke();
                }
            }
        }
    }


    public void Move(float move, bool crouch, bool jump)
    {
        // If crouch has just been let go of
        if (!crouch)
        {
            if(isCrouching)
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(headPos.position, collisionCheckRadius, LayerMask.GetMask("Ground")))
                {
                    crouch = true;
                }
            }
        }

        if (isGrounded || allowAirControl)
        {
            // Crouching
            if (crouch)
            {
                // Player has just crouched this frame
                if (!isCrouching)
                {
                    isCrouching = true;
                    OnPlayerCrouch.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= (1 - crouch_speed_reduction);

                // Disable the top collider when crouching
                if (mainCollider != null)
                {
                    mainCollider.enabled = false;
                }
            }
            // Not crouching
            else
            {
                // Enable the collider when not crouching
                if (mainCollider != null)
                {
                    mainCollider.enabled = true;
                }

                // Player has just uncrouched
                if (isCrouching)
                {
                    isCrouching = false;
                    OnPlayerCrouch.Invoke(false);
                }
            }


            // Move the character by finding the target velocity
            Vector2 targetVelocity = new Vector2(move * 10f, rigid.velocity.y);

            // And then smoothing it out and applying it to the character
            rigid.velocity = Vector2.SmoothDamp(rigid.velocity, targetVelocity, ref velocity, movementSmoothing);

            // If the input is moving the player right and the player is facing left
            if (move > 0 && !facing.Equals(Direction.Right))
            {
                // Face right
                facing = Direction.Right;
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right
            else if (move < 0 && !facing.Equals(Direction.Left))
            {
                // Face left
                facing = Direction.Left;
                Flip();
            }
        }

        // Regular jump
        if (isGrounded && jump && !crouch)
        {
            // Add a vertical force to the player.
            isGrounded = false;
            Jump(jump_force);
        }
        // Crouch jump
        else if (isGrounded && jump && crouch)
        {
            // Jump forwards
            // TODO
            isGrounded = false;
            float force = 2 * jump_force;
            if(facing == Direction.Left)
            {
                force *= -1;
            } 
            else if(facing == Direction.Forward)
            {
                force = 0;
            }
            ForwardJump(force, jump_force);
        }
        // Double jump
        else if (!isGrounded && jump && double_jumps_left > 0)
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
        rigid.AddForce(new Vector2(0, force));
    }


    private void ForwardJump(float forceX, float forceY)
    {
        // Set y vel to 0
        rigid.velocity = Vector2.zero;

        // Then jump
        rigid.AddForce(new Vector2(forceX, forceY));
    }


    private void Flip()
    {
        // Multiply the player's x local scale by -1.
        /*
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        */
        // Will use this next time
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(headPos.position, collisionCheckRadius);
        Gizmos.DrawSphere(feetPos.position, collisionCheckRadius);
    }



}
