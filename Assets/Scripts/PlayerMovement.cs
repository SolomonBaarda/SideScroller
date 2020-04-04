using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocity settings")]
    public float max_speed = 6f;
    public float terminal_velocity = 20f;

    [Header("Jump settings")]
    public float jump_power = 6f;
    public int max_double_jumps = 1;
    public int double_jumps_left = 0;

    private Vector2 velocity;

    [HideInInspector]
    public Keys keys;

    public bool inputIsAllowed;


    public class Keys
    {
        public KeyCode left, right, up, down, jump, interact, slow;
    }


    public enum Direction { Left, Right, Forward };
    private Direction facing = Direction.Forward;

    // For animations only
    private enum JumpState { up, peak, down, none };
    private JumpState jumpState;


    private void Awake()
    {
        velocity = new Vector2();

        // Set the keys
        keys = new Keys();
        keys.left = KeyCode.A;
        keys.right = KeyCode.D;
        keys.up = KeyCode.W;
        keys.down = KeyCode.S;
        keys.jump = KeyCode.Space;
        keys.interact = KeyCode.E;
        keys.slow = KeyCode.LeftShift;

        inputIsAllowed = true;
    }

    private void FixedUpdate()
    {
        bool isInAir = false;

        BoxCollider2D b = GetComponentInChildren<BoxCollider2D>();

        // If touching the ground
        if (b.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            // Reset double jump
            double_jumps_left = max_double_jumps;

            velocity.x = 0;
            isInAir = false;

            if (inputIsAllowed)
            {
                // Do full jump 
                if (Input.GetKey(keys.jump))
                {
                    Jump(jump_power);
                }
            }
        }
        else if (b.IsTouchingLayers(LayerMask.GetMask("Wall")))
        {
            // Reset double jump
            double_jumps_left = max_double_jumps;

            velocity.x = 0;
            isInAir = false;

            if (inputIsAllowed)
            {
                // Do wall jump
                if (Input.GetKey(keys.jump))
                {
                    WallJump(jump_power / 10, jump_power);
                }
            }
        }
        // Not on ground 
        else
        {
            isInAir = true;

            if (inputIsAllowed)
            {
                // Do double jump if possible
                if (Input.GetKeyDown(keys.jump))
                {
                    if (double_jumps_left > 0)
                    {
                        double_jumps_left--;
                        Jump(jump_power);
                    }
                }
            }
        }

        bool wasLeft = false;
        bool wasRight = false;

        if (inputIsAllowed)
        {
            // Move left and right
            if (Input.GetKey(keys.left))
            {
                wasLeft = true;
            }
            if (Input.GetKey(keys.right))
            {
                wasRight = true;
            }
            // Both keys pressed
            if (Input.GetKey(keys.down))
            {
                wasLeft = true;
                wasRight = true;
            }
        }

        // Velocity should be 0 by default
        velocity.x = 0;

        // Update if player has moved
        // Only pressed left
        if (wasLeft && !wasRight)
        {
            facing = Direction.Left;
            velocity.x = -max_speed * Time.deltaTime;
        }
        // Only pressed right
        else if (!wasLeft && wasRight)
        {
            facing = Direction.Right;
            velocity.x = max_speed * Time.deltaTime;
        }
        // Pressed both keys
        else if (wasLeft && wasRight)
        {
            facing = Direction.Forward;
        }
        else
        {
            // Do nothing
            // This keeps the player facing in the direction they last moved
        }

        // Ensure velocity is capped and then apply it
        velocity.x = Mathf.Clamp(velocity.x, -max_speed, max_speed);
        transform.Translate(velocity, Space.World);


        // Clamp the max velocities
        /*
        Vector2 v = GetComponent<Rigidbody2D>().velocity;
        v.y = Mathf.Clamp(v.y, -terminal_velocity, terminal_velocity);
        v.x = Mathf.Clamp(v.x, -max_speed, max_speed);
        GetComponent<Rigidbody2D>().velocity = v;
        */


        // Update animations
        Animator a = GetComponentInChildren<Animator>();
        a.SetBool("isJumping", isInAir);
        a.SetBool("isRight", facing.Equals(Direction.Right));
        a.SetBool("isLeft", facing.Equals(Direction.Left));
        a.SetBool("isForward", facing.Equals(Direction.Forward));
        a.SetFloat("velocityY", GetComponent<Rigidbody2D>().velocity.y);
    }

    private void OnEnable()
    {
        // Face forward by default
        facing = Direction.Forward;
    }



    private void Jump(float power)
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();

        // Reset the y velocity of the player
        //Vector2 v = r.velocity;
        //v.y = 0;
        r.velocity = Vector2.zero;

        // Then jump
        r.AddForce(new Vector2(0, power), ForceMode2D.Impulse);
    }


    private void WallJump(float xPower, float yPower)
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();

        // Reset the y velocity of the player
        r.velocity = Vector2.zero;

        // Jump right
        if (facing.Equals(Direction.Left))
        {
            facing = Direction.Right;
            Leap(xPower, yPower);
        }
        // Jump left
        else if (facing.Equals(Direction.Right))
        {
            facing = Direction.Left;
            Leap(-xPower, yPower);
        }
        // Jump up
        else
        {
            Jump(yPower);
        }

    }


    private void Leap(float xPower, float yPower)
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();

        // Reset the y velocity of the player
        r.velocity = Vector2.zero;

        r.AddForce(new Vector2(xPower, yPower), ForceMode2D.Impulse);
    }


    private float MoveTowards(float value, float target, float amount)
    {
        if (value < target)
        {
            value += amount;
            if (value > target)
            {
                value = target;
            }
        }
        else if (value > target)
        {
            value -= amount;
            if (value < target)
            {
                value = target;
            }
        }

        return value;
    }

}
