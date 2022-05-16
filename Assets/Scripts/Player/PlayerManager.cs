using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputHandler inputHandler;
    [SerializeField]
    CameraHandler cameraHandler;
    PlayerLocomotion playerLocomotion;
    Animator animator;
    public bool isInteracting;

    [Header ("Player Actions")]
    public bool isRun;
    public bool isInAir;
    public bool isGround;

    void Start()
    {
        inputHandler = GetComponent<InputHandler>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        isInteracting = animator.GetBool("isInteracting");

        float delta = Time.deltaTime;
        inputHandler.TickInput(delta);

        playerLocomotion.HandleMovement(delta);

        playerLocomotion.RollAndJump(delta);

    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        if (cameraHandler != null)
        {
            cameraHandler.FollowTarget(delta);
            cameraHandler.CameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
        }

        playerLocomotion.Fall(delta);

        playerLocomotion.StepClimb(delta);
    }

    private void LateUpdate()
    {
        inputHandler.roll = false;
        inputHandler.run = false;

        if (isInAir)
        {
            playerLocomotion.inAirTimer = playerLocomotion.inAirTimer + Time.deltaTime;
        }
    }
}
