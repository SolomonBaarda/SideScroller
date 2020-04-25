using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInteraction interaction;
    private PlayerInventory inventory;

    private float horizontalMovement = 0f;
    private bool isJump = false;
    private bool isCrouch = false;

    private bool isInteract1 = false;
    private bool isInteract2 = false;

    private float scrollAmount = 0f;
    private bool isFire1 = false;
    private bool isFire2 = false;
    private bool isFire3 = false;

    private void Awake()
    {
        // Get reference to the controller script
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        // Update the control values
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        isJump = Input.GetButtonDown("Jump");
        isCrouch = Input.GetButton("Crouch");

        isInteract1 = Input.GetButton("Interact");
        isInteract2 = Input.GetButton("Interact");

        scrollAmount = Input.GetAxisRaw("Mouse ScrollWheel");
        isFire1 = Input.GetButton("Fire1");
        isFire2 = Input.GetButton("Fire2");
        isFire3 = Input.GetButton("Fire3");

    }

    private void FixedUpdate()
    {
        // Interact with items
        interaction.Interact(isInteract1);

        inventory.DropWeapon(isInteract2);
        Buff currentTotal = inventory.GetCurrentTotal();

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

}
