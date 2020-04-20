using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    //[SerializeField] private float max_speed = 6f;
    [SerializeField] private float jump_force = 400f;
    [SerializeField] private int max_double_jumps = 1;
    [SerializeField] private int double_jumps_left = 0;
    [Range(0, 1)] [SerializeField] private float crouch_speed_reduction = .4f;

    [Header("Controller Settings")]
    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f;
    [SerializeField] private bool allowAirControl = true;
    private const float collisionCheckRadius = .2f;
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
                if (!wasGrounded)
                {
                    OnPlayerland.Invoke();
                }
            }
        }
    }


    public void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(headPos.position, collisionCheckRadius, LayerMask.GetMask("Ground")))
            {
                crouch = true;
            }
        }

        // only control the player if grounded or airControl is turned on
        if (isGrounded || allowAirControl)
        {
            // Player has just crouched
            if (crouch)
            {
                if (!isCrouching)
                {
                    isCrouching = true;
                    OnPlayerCrouch.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= crouch_speed_reduction;

                // Disable one of the colliders when crouching
                if (mainCollider != null)
                {
                    //mainCollider.enabled = false;
                }
            }
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

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !facing.Equals(Direction.Right))
            {
                facing = Direction.Right;
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && !facing.Equals(Direction.Left))
            {
                facing = Direction.Left;
                Flip();
            }
        }
        // If the player should jump...
        if (isGrounded && jump)
        {
            // Add a vertical force to the player.
            isGrounded = false;
            rigid.AddForce(new Vector2(0f, jump_force));
        }
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
