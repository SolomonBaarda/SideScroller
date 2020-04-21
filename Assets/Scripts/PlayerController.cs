using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInteraction interaction;

    private float horizontalMovement = 0f;
    private bool isJump = false;
    private bool isCrouch = false;
    private bool isInteract = false;



    private void Awake()
    {
        // Get reference to the controller script
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
    }

    private void Update()
    {
        // Update the control values
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        isJump = Input.GetButtonDown("Jump");
        isCrouch = Input.GetButton("Crouch");
        isInteract = Input.GetButton("Interact");
    }

    private void FixedUpdate()
    {
        // Move first
        movement.Move(horizontalMovement * Time.fixedDeltaTime, isCrouch, isJump);
        isJump = false;

        interaction.Interact(isInteract);
    }



    private void OnEnable()
    {
        movement.enabled = true;
    }

    private void OnDisable()
    {
        movement.enabled = false;
    }

}
