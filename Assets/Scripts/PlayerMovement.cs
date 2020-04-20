using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController controller;

    public float speed = 40f;

    private float horizontalMovement = 0f;
    private bool isJump = false;
    private bool isCrouch = false;

    [HideInInspector]
    //public Keys keys;

    public class Keys
    {
        public KeyCode left, right, up, down, jump, interact1, interact2, slow, escape;
    }

    private void Awake()
    {
        // Get reference to the controller script
        controller = GetComponent<PlayerController>();

        // Set the keys
        /*
        keys = new Keys();
        keys.left = KeyCode.A;
        keys.right = KeyCode.D;
        keys.up = KeyCode.W;
        keys.down = KeyCode.S;
        keys.jump = KeyCode.Space;
        keys.interact1 = KeyCode.E;
        keys.interact2 = KeyCode.Q;
        keys.slow = KeyCode.LeftShift;
        keys.escape = KeyCode.Escape;
        */
    }



    private void OnEnable()
    {
        controller.enabled = true;
    }



    private void OnDisable()
    {
        controller.enabled = false;
    }



    private void Update()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal") * speed;
        isJump = Input.GetButtonDown("Jump");
        isCrouch = Input.GetButton("Crouch");

    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMovement * Time.fixedDeltaTime, isCrouch, isJump);
        isJump = false;
    }



}
