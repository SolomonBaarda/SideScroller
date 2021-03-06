﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInteraction interaction;
    private PlayerInventory inventory;

    private Animator animator;

    private float horizontalMovement = 0f;
    private float verticalMovement = 0f;
    private bool isJump = false;
    private bool isCrouch = false;

    private bool isInteract1 = false;
    private bool isInteract2 = false;

    bool canUseController;
    private string player_axis_prefix = "";
    private const string controller_axis_prefix = "C";

    private void Awake()
    {
        // Get reference to the controller script
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
        inventory = GetComponent<PlayerInventory>();

        animator = GetComponent<Animator>();
    }


    public void SetControls(string ID, bool canUseController)
    {
        player_axis_prefix = ID;
        this.canUseController = canUseController;
    }


    private bool GetButton(string prefix, string button)
    {
        return Input.GetButton(prefix + "_" + button);
    }

    private bool GetButtonDown(string prefix, string button)
    {
        return Input.GetButtonDown(prefix + "_" + button);
    }

    private float GetAxisRaw(string prefix, string axis)
    {
        return Input.GetAxisRaw(prefix + "_" + axis);
    }


    /// <summary>
    /// Returns the float with the largest absolute value.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float GetMax(float a, float b)
    {
        if(Mathf.Abs(a) > Mathf.Abs(b))
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    private bool GetMax(bool a, bool b)
    {
        return a || b;
    }


    private void Update()
    {
        // Update the control values
        horizontalMovement = GetAxisRaw(player_axis_prefix, "Horizontal");
        verticalMovement = GetAxisRaw(player_axis_prefix, "Vertical");

        isJump = GetButtonDown(player_axis_prefix, "Jump");
        isCrouch = GetButton(player_axis_prefix, "Crouch");

        isInteract1 = GetButton(player_axis_prefix, "Interact");
        isInteract2 = GetButton(player_axis_prefix, "Interact2");

        // Check controller values as well and update them if we need to 
        if(canUseController)
        {
            // Choose the maximum of both inputs
            horizontalMovement = GetMax(GetAxisRaw(controller_axis_prefix, "Horizontal"), horizontalMovement);
            verticalMovement = GetMax(GetAxisRaw(controller_axis_prefix, "Vertical"), verticalMovement);

            isJump = GetMax(GetButtonDown(controller_axis_prefix, "Jump"), isJump);
            isCrouch = GetMax(GetButton(controller_axis_prefix, "Crouch"), isCrouch);

            isInteract1 = GetMax(GetButton(controller_axis_prefix, "Interact"), isInteract1);
            isInteract2 = GetMax(GetButton(controller_axis_prefix, "Interact2"), isInteract2);
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        bool jump = isJump, crouch = isCrouch;
        isJump = false;

        // Move weapon first
        // If the weapon didn't move
        if(!interaction.UpdateHandAndWeaponPosition(verticalMovement))
        {
            // Update the crouch and jump values as well (disable for now)
            //jump = GetMax(jump, verticalMovement < 0);
            //crouch = GetMax(crouch, verticalMovement > 0);
        }
        
        // Then attack 
        interaction.Attack(isInteract2);
        // Then interact with items
        interaction.Interact(isInteract1);

        inventory.UpdateHeldItemPosition();

        // Move last
        movement.Move(horizontalMovement * Time.fixedDeltaTime, crouch, jump);
    }


    private void UpdateAnimations()
    {
        // X velocity is always positive, just flipped x on local scale
        animator.SetFloat("VelocityX", Mathf.Abs(movement.PlayerVelocity.x));
        animator.SetFloat("VelocityY", movement.PlayerVelocity.y);
        animator.SetBool("isJumping", !movement.IsOnGround);
        animator.SetBool("isCrouching", movement.IsCrouching);
    }

}
