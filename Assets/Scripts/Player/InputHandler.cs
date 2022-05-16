using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public float vertical;
    public float horizontal;
    public float moveAmount;
    public float mouseX;
    public float mouseY;

    public bool roll_input;
    public bool roll;
    public bool jump_input;
    public bool jump;
    public float InputTimer;
    public bool run_input;
    public bool run;

    PlayerController inputActions;
    AnimatorHandler animatorHandler;

    Vector2 movementInput;
    Vector2 cameraInput;


    private void Start()
    {
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
    }

    public void OnEnable()
    {
        if(inputActions == null)
        {
            inputActions = new PlayerController();
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
        MoveInput(delta);
        RollInput(delta);
    }

    private void MoveInput(float delta)
    {
        run_input = inputActions.PlayerAction.Run.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        if (run_input)
        {
            run = true;
        }
        else
        {
            run = false;
        }
        horizontal = movementInput.x;
        vertical = movementInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        mouseX = cameraInput.x;
        mouseY = cameraInput.y;
    }

    public void RollInput(float delta)
    {
        roll_input = inputActions.PlayerAction.Roll.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        if (roll_input)
        {
            InputTimer += delta;
            roll = true;
            animatorHandler.StopRotate();
        }
        else
        {
            if(InputTimer > 0 && InputTimer < 0.75f)
            {
                roll = true;
            }
            else
            {
                roll = false;
                animatorHandler.CanRotate();
            }

            InputTimer = 0;
        }
    }

    public void JumpInput(float delta)
    {
        jump_input = inputActions.PlayerAction.Jump.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        if (jump_input)
        {
            InputTimer += delta;
            jump = true;
            animatorHandler.StopRotate();
        }
        else
        {
            if (InputTimer > 0 && InputTimer < 0.5f)
            {
                jump = true;
            }
            else
            {
                jump = false;
                animatorHandler.CanRotate();
            }

            InputTimer = 0;
        }
    }
}
