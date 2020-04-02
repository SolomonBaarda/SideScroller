using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float max_speed = 6f;
    public float terminal_velocity = 20f;

    [Space(8)]
    public float jump_power = 6f;
    public int max_double_jumps = 1;
    public int double_jumps_left = 0;

    private Vector2 velocity;

    [HideInInspector]
    public Keys keys;


    public class Keys
    {
        public KeyCode left, right, up, down, jump, interact, slow;
    }


    public enum Direction { Left, Right };
    private Direction facing;
    private enum JumpState { none, up1, up2, land1, land2, land3 };
    private JumpState jumpState;

    private void Awake()
    {


        velocity = new Vector2();

        keys = new Keys();
        keys.left = KeyCode.A;
        keys.right = KeyCode.D;
        keys.up = KeyCode.W;
        keys.down = KeyCode.S;
        keys.jump = KeyCode.Space;
        keys.interact = KeyCode.E;
        keys.slow = KeyCode.LeftShift;

        facing = Direction.Right;
    }

    private void FixedUpdate()
    {
        bool isInAir = false;

        // If touching the ground
        if (transform.GetComponentInChildren<BoxCollider2D>().IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            // Reset double jump
            double_jumps_left = max_double_jumps;

            velocity.x = 0;
            isInAir = false;

            // Do full jump 
            if (Input.GetKey(keys.jump))
            {
                Jump(jump_power);
            }
        }
        else if (transform.GetComponentInChildren<BoxCollider2D>().IsTouchingLayers(LayerMask.GetMask("Wall")))
        {
            // Reset double jump
            double_jumps_left = max_double_jumps;

            velocity.x = 0;
            isInAir = false;

            // Do wall jump
            if (Input.GetKey(keys.jump))
            {
                WallJump(jump_power / 10, jump_power);
            }
        }
        // Not on ground 
        else
        {
            isInAir = true;

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

        bool wasLeft = false;
        bool wasRight = false;
        // Move left and right
        if (Input.GetKey(keys.left))
        {
            facing = Direction.Left;
            wasLeft = true;
        }
        if (Input.GetKey(keys.right))
        {
            facing = Direction.Right;
            wasRight = true;
        }
        // Look down (stop)
        if (Input.GetKey(keys.down))
        {
            wasLeft = true;
            wasRight = true;
        }

        if (wasLeft && !wasRight)
        {
            velocity.x = -max_speed * Time.deltaTime;
        }
        else if (!wasLeft && wasRight)
        {
            velocity.x = max_speed * Time.deltaTime;
        }
        else if (wasLeft && wasRight)
        {
            velocity.x = 0;
        }

        // Ensure velocity is capped and then apply it
        velocity.x = Mathf.Clamp(velocity.x, -max_speed, max_speed);
        transform.Translate(velocity, Space.World);

        Vector2 v = GetComponent<Rigidbody2D>().velocity;
        v.y = Mathf.Clamp(v.y, -terminal_velocity, terminal_velocity);
        v.x = Mathf.Clamp(v.x, -max_speed, max_speed);
        GetComponent<Rigidbody2D>().velocity = v;


        Animator a = GetComponentInChildren<Animator>();
        a.SetBool("isJumping", isInAir);
        //a.SetBool("isForward", facing.Equals(Direction.Forward));
        a.SetBool("isRight", facing.Equals(Direction.Right));
        a.SetBool("isLeft", facing.Equals(Direction.Left));
        a.SetFloat("velocityX", velocity.x);
        a.SetFloat("velocityY", GetComponent<Rigidbody2D>().velocity.y);
    }

    private void Jump(float power)
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();

        // Reset the y velocity of the player
        Vector2 v = r.velocity;
        v.y = 0;
        r.velocity = v;

        // Then jump
        r.AddForce(new Vector2(0, power), ForceMode2D.Impulse);
    }


    private void WallJump(float xPower, float yPower)
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();

        // Reset the y velocity of the player
        Vector2 v = r.velocity;
        v.y = 0;
        r.velocity = v;

        int direction = 0;
        if (facing.Equals(Direction.Left))
        {
            direction = 1;
            facing = Direction.Right;
        }
        else if (facing.Equals(Direction.Right))
        {
            direction = -1;
            facing = Direction.Left;
        }

        // Then jump
        r.AddForce(new Vector2(direction * xPower, yPower), ForceMode2D.Impulse);
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
