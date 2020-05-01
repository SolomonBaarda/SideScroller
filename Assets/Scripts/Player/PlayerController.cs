using System.Collections;
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

    private float scrollAmount = 0f;
    private bool isFire1 = false;
    private bool isFire2 = false;
    private bool isFire3 = false;

    private string prefix = "";

    private void Awake()
    {
        // Get reference to the controller script
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
        inventory = GetComponent<PlayerInventory>();

        animator = GetComponent<Animator>();
    }


    public void SetPlayer(string ID)
    {
        prefix = ID + "_";
    }

    private void Update()
    {
        // Update the control values
        horizontalMovement = Input.GetAxisRaw(prefix + "Horizontal");
        verticalMovement = Input.GetAxisRaw(prefix + "Vertical");
        isJump = Input.GetButtonDown(prefix + "Jump");
        isCrouch = Input.GetButton(prefix + "Crouch");

        isInteract1 = Input.GetButton(prefix + "Interact");
        isInteract2 = Input.GetButton(prefix + "Interact2");

        //scrollAmount = Input.GetAxisRaw("Mouse ScrollWheel");
        //isFire1 = Input.GetButton("Fire1");
        //isFire2 = Input.GetButton("Fire2");
        //isFire3 = Input.GetButton("Fire3");

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Interact with items
        interaction.Interact(isInteract1);

        inventory.DropWeapon(isInteract2);
        //Buff currentTotal = inventory.GetCurrentTotal();

        // Update the inventory and attack
        inventory.Attack(scrollAmount, isFire1, isFire2, movement.PlayerVelocity);

        // Move last
        movement.Move(horizontalMovement * Time.fixedDeltaTime, isCrouch, isJump);
        isJump = false;
    }



    private void OnEnable()
    {
        movement.enabled = true;
        interaction.enabled = true;
    }

    private void OnDisable()
    {
        movement.enabled = false;
        interaction.enabled = false;
    }


    private void UpdateAnimations()
    {
        animator.SetFloat("VelocityX", Mathf.Abs(movement.PlayerVelocity.x));
        animator.SetFloat("VelocityY", movement.PlayerVelocity.y);
        animator.SetBool("isJumping", !movement.IsOnGround);
        animator.SetBool("isCrouching", movement.IsCrouching);
    }

}
