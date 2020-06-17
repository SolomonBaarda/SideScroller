using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = SpeedPreset.Default;
    public static Presets.VariableValue<int> SpeedPreset = new Presets.VariableValue<int>(40, 30, 50, 15, 80);
    [SerializeField] private float jump_force = 12;
    [SerializeField] private int default_double_jumps = 1;
    [SerializeField] private int max_double_jumps = 0;
    [SerializeField] private int double_jumps_left = 0;
    [Range(0, 1)] [SerializeField] private float crouch_speed_reduction = .4f;

    [Header("Controller Settings")]
    [Range(0, .5f)] [SerializeField] private float movementSmoothing = .02f;
    [SerializeField] private bool allowAirControl = true;

    public readonly Vector2 TERMINAL_VELOCITY = new Vector2(16, 124);
    private Vector2 previousVelocity = Vector2.zero;
    private bool previouslyGrounded = false;
    private bool previouslySliding = false;
    private const float minimum_slide_time = 0.5f;
    private float slideTime = 0;
    private bool previouslyCrouchWalking = false;

    public Player.Facing Facing { get; private set; } = Player.Facing.Right;

    public Vector2 PlayerVelocity { get { return rigid.velocity; } }

    public bool IsCrouching { get; private set; }
    public bool IsOnGround { get; private set; }

    private Rigidbody2D rigid;
    private Collider2D torsoCollider;
    private Transform headPos;
    private Transform feetPos;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        max_double_jumps = default_double_jumps;
    }


    public void Initialise(float speed, Collider2D torsoCollider, Transform headPosition, Transform feetPosition)
    {
        this.speed = speed;

        this.torsoCollider = torsoCollider;

        headPos = headPosition;
        feetPos = feetPosition;
    }


    public void ResetVelocity()
    {
        rigid.velocity = Vector2.zero;
    }

    private void OnDisable()
    {
        ResetVelocity();
    }


    private void FixedUpdate()
    {
        // Check if the player is on the ground
        previouslyGrounded = IsOnGround;
        IsOnGround = false;

        // Check ground collisions
        if (GroundCheck.IsOnGround(feetPos.position))
        {
            IsOnGround = true;
            double_jumps_left = max_double_jumps;
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
                if(GroundCheck.IsOnGround(headPos.position))
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
                if (torsoCollider != null)
                {
                    torsoCollider.enabled = false;
                }
            }
            // Set not crouching
            else
            {
                // Enable the collider when not crouching
                if (torsoCollider != null)
                {
                    torsoCollider.enabled = true;
                }

                // Player has just uncrouched
                if (IsCrouching)
                {
                    IsCrouching = false;
                }
            }



            // Movement stuff

            bool isSliding = false;
            bool isCrouchWalking = false;

            // Player is crouching, overwrite target vel
            if (IsCrouching && IsOnGround)
            {
                // Player has just landed from falling
                /*
                if (!previouslyGrounded)
                {
                    // Do a big slide in the direction the player is facing

                    // Get the direction
                    int dir = 0;
                    if (facing.Equals(Direction.Right)) { dir = 1; }
                    else if (facing.Equals(Direction.Left)) { dir = -1; }

                    // Transfer the y velocity into x
                    Vector2 newVelocity = rigid.velocity;
                    newVelocity.x = dir * Mathf.Abs(previousVelocity.y);
                    rigid.velocity = newVelocity;

                    // Update timer and conditions
                    isSliding = true;
                    slideTime = 0;
                }
                */

                
                // Player was already on the ground
                if(previouslyGrounded)
                {
                    // Player wants to start crouch walking
                    bool slidingJustEnded = previouslySliding && slideTime >= minimum_slide_time && move != 0;

                    // Player is crouch waling 
                    if (slidingJustEnded || previouslyCrouchWalking)
                    {
                        isCrouchWalking = true;
                        isSliding = false;
                    }

                    // If not crouch walking, must be sliding
                    if (!isCrouchWalking)
                    {
                        isSliding = true;
                    }
                }

                // Record the values for use next frame
                previouslyCrouchWalking = isCrouchWalking;
                previouslySliding = isSliding;
            }

            if (!IsOnGround)
            {
                isCrouchWalking = false;
                isSliding = false;
            }

            // Set velocity

            // Set crouch walk speed
            if (isCrouchWalking)
            {
                move *= (1 - crouch_speed_reduction);
            }

            // Desired velocity
            Vector2 targetVelocity = new Vector2(move * 10, rigid.velocity.y);

            // Slow down when sliding
            if (isSliding)
            {
                // Reduce the speed by the crouch speed reduction
                targetVelocity.x = Mathf.MoveTowards(rigid.velocity.x, 0, crouch_speed_reduction);
                slideTime += Time.deltaTime;
            }
            // Reset timer if not sliding
            else
            {
                slideTime = minimum_slide_time;
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
                if (move > 0 && !Facing.Equals(Player.Facing.Right))
                {
                    // Face right
                    Face(Player.Facing.Right);
                }
                // Otherwise if the input is moving the player left and the player is facing right
                else if (move < 0 && !Facing.Equals(Player.Facing.Left))
                {
                    // Face left
                    Face(Player.Facing.Left);
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


    public void Face(Player.Facing d)
    {
        Facing = d;

        // Flip the player
        Vector3 scale = transform.localScale;

        int i = 0;
        if (d == Player.Facing.Left)
        {
            i = -1;
        }
        else if (d == Player.Facing.Right)
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


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(headPos.position, GroundCheck.DEFAULT_RADIUS);
        Gizmos.DrawSphere(feetPos.position, GroundCheck.DEFAULT_RADIUS);
    }



}
