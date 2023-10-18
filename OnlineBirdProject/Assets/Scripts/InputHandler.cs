using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public float horizontal;
    public float vertical;
    public float moveAmount;
    public float mouseX;
    public float mouseY;

    public bool interact_input;
    public bool sprint_input;
    public bool s_input;

    public bool InteractFlag;
    public bool sprintFlag;

    PlayerControls inputActions;

    Vector2 movementInput;
    Vector2 cameraInput;



    // Bird specific
    public bool jump_input;
    public bool jumpHold_input;
    public bool fly_input;

    public bool tiltRight;
    public bool tiltLeft;

    public void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerControls();
            inputActions.PlayerMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
            inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
        }

        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public void TickInput(float delta)
    {
        HandleMoveInput(delta);
        HandleJumpInput(delta);
        HandleTiltInput(delta);
        HandleSprintInput(delta);
        HandleInteractInput(delta);
    }

    private void HandleTiltInput(float delta)
    {
        tiltRight = inputActions.PlayerActions.TiltRight.IsPressed();
        tiltLeft = inputActions.PlayerActions.TiltLeft.IsPressed();
    }

    private void HandleJumpInput(float delta)
    {
        jump_input = inputActions.PlayerActions.Jump.WasPressedThisFrame();
        jumpHold_input = inputActions.PlayerActions.Jump.IsPressed();
        fly_input = jumpHold_input;
    }

    private void HandleMoveInput(float delta)
    {
        // Movimiento mas smooth
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //horizontal = movementInput.x;
        //vertical = movementInput.y;
        //moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        //mouseX = cameraInput.x;
        //mouseY = cameraInput.y;
    }

    private void HandleSprintInput(float delta)
    {
        sprint_input = inputActions.PlayerActions.Sprint.IsPressed();

        if (sprint_input)
            sprintFlag = true;
    }

    private void HandleInteractInput(float delta)
    {
        interact_input = inputActions.PlayerActions.Interact.WasPressedThisFrame();

        if (interact_input)
            InteractFlag = true;
    }

    public void ResetMovementValues()
    {
        horizontal = 0;
        vertical = 0;
    }
}