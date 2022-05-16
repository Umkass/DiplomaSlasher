using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{

    Transform cameraTransform;
    InputHandler inputHandler;
    public Vector3 moveDirection;

    [HideInInspector]
    Transform myTransform;
    [HideInInspector]
    AnimatorHandler animatorHandler;
    PlayerManager playerManager;

    public new Rigidbody rigidbody;
    public GameObject normalCamera;

    [Header("Ground Detect")]
    [SerializeField]
    float groundDetectRayStartPoint = 0f;
    [SerializeField]
    float minDistanceToFall = 0.25f;
    [SerializeField]
    float distanceToLand = 0.1f;
    [SerializeField]
    float groundDirectionRayDistance = 0.25f;
    LayerMask ignoreGroundCheck;
    public float inAirTimer;

    [Header("Ground Detect")]
    [SerializeField]
    GameObject stepRayUpper;
    [SerializeField]
    GameObject stepRayLower;
    [SerializeField]
    float stepHeight = 0.15f;
    [SerializeField]
    float stepSmooth = 0.15f;

    [Header("Movement Stats")]

    [SerializeField]
    float movementSpeed = 1f;
    [SerializeField]
    float runSpeed = 2f;
    [SerializeField]
    float rotationSpeed = 5f;
    [SerializeField]
    float fallingSpeed = 10f;


    void Awake()
    {
        stepRayUpper.transform.localPosition = new Vector3(stepRayUpper.transform.localPosition.x,stepHeight,stepRayUpper.transform.localPosition.z);
    }
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        playerManager = GetComponent<PlayerManager>();
        cameraTransform = Camera.main.transform;
        myTransform = transform;
        animatorHandler.Initialize();

        playerManager.isGround = true;
        ignoreGroundCheck = ~(1 << 8 | 1 << 11);
    }



    #region Movement

    Vector3 normalVector;
    Vector3 targetPosition;

    public void HandleRotation(float delta)
    {
        Vector3 targetDir = Vector3.zero;
        float moveOverride = inputHandler.moveAmount;
        targetDir = cameraTransform.forward * inputHandler.vertical;
        targetDir += cameraTransform.right * inputHandler.horizontal;

        targetDir.Normalize();
        targetDir.y = 0;
        if (targetDir == Vector3.zero)
            targetDir = myTransform.forward;

        float rs = rotationSpeed;

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
        myTransform.rotation = targetRotation;
    }

    public void HandleMovement(float delta)
    {
        if (inputHandler.roll)
        {
            //animatorHandler.anim.SetFloat("Vertical", 0);
            return;
        }

        if (playerManager.isInteracting)
            return;
        moveDirection = cameraTransform.forward * inputHandler.vertical;
        moveDirection += cameraTransform.right * inputHandler.horizontal;
        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = movementSpeed;

        if (inputHandler.run)
        {
            speed = runSpeed;
            playerManager.isRun = true;
            moveDirection *= speed;
        }
        else
        {
            playerManager.isRun = false;
            moveDirection *= speed;
        }
        
        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
        rigidbody.velocity = projectedVelocity;

        //if (playerManager.isInteracting)
        //    return;
        animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isRun);
        if (animatorHandler.canRotate)
        {
            HandleRotation(delta);
        }
    }

    public void RollAndJump(float delta)
    {
        if (animatorHandler.anim.GetBool("isInteracting"))
            return;
        if (inputHandler.roll)
        {
            moveDirection = cameraTransform.forward * inputHandler.vertical;
            moveDirection += cameraTransform.right * inputHandler.horizontal;

            if(inputHandler.moveAmount > 0) 
            {
                if (playerManager.isRun)
                    animatorHandler.anim.speed = 1.35f;
                animatorHandler.PlayTargetAnimation("Roll", true);
                moveDirection.y = 0;
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                myTransform.rotation = rollRotation;
            }
        }

        if (inputHandler.jump)
        {
            //moveDirection = cameraTransform.forward * inputHandler.vertical;
            //moveDirection += cameraTransform.right * inputHandler.horizontal;
            //animatorHandler.PlayTargetAnimation("Jump", true);
            //moveDirection.y = 0;
            //Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
            //myTransform.rotation = rollRotation;
        }
    }

    public void Fall(float delta)
    {
        playerManager.isGround = false;
        RaycastHit hit;
        Vector3 origin = myTransform.position;
        origin.y += groundDetectRayStartPoint;
        
        if (playerManager.isInAir)
        {
            rigidbody.AddForce(-Vector3.up * fallingSpeed);
        }

        //Vector3 dir = moveDirection;
        //Debug.Log("O " + origin);
        //dir.Normalize();
        //origin = origin + -dir * groundDirectionRayDistance;
        //Debug.Log("O " + origin);

        Debug.DrawRay(origin, -Vector3.up * minDistanceToFall, Color.red, minDistanceToFall, false);
        Debug.DrawRay(origin, -Vector3.up * distanceToLand, Color.green, distanceToLand, false);

        if (Physics.Raycast(origin, -Vector3.up, out hit, minDistanceToFall, ignoreGroundCheck))
        {
            normalVector = hit.normal;
            playerManager.isGround = true;

            if (playerManager.isInAir)
            {
                if(inAirTimer>0)
                {
                    if (Physics.Raycast(origin, -Vector3.up, out hit, distanceToLand, ignoreGroundCheck))
                    {
                        Debug.Log("inAir " + inAirTimer);
                        animatorHandler.PlayTargetAnimation("Land", true);
                        inAirTimer = 0;
                        playerManager.isInAir = false;
                    }
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("Locomotion", false);
                    inAirTimer = 0;
                    playerManager.isInAir = false;
                }
            }
        }
        else
        {
            if (playerManager.isGround)
            {
                playerManager.isGround = false;
            }
            if (!playerManager.isInAir)
            {


                playerManager.isInAir = true;
                //if (myTransform.position.x != origin.x)
                //    myTransform.position = origin;
            }
            else
            {
                if (!playerManager.isInteracting)
                {
                    if (inAirTimer > 0.1f)
                    {
                        Debug.Log("START FALL");
                        animatorHandler.PlayTargetAnimation("Fall", true);
                        Vector3 velocity = rigidbody.velocity;
                        velocity.Normalize();
                        rigidbody.velocity = velocity * (movementSpeed / 2f);
                        myTransform.position += moveDirection * 0.05f;
                    }
                }
            }

        }
    }

    public void StepClimb(float delta)
    {
        Debug.DrawRay(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), Color.yellow, 0.1f, false);
        Debug.DrawRay(stepRayLower.transform.position, transform.TransformDirection(1.5f, 0, 1), Color.yellow, 0.1f, false);
        Debug.DrawRay(stepRayLower.transform.position, transform.TransformDirection(-1.5f, 0, 1), Color.yellow, 0.1f, false);
        Debug.DrawRay(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), Color.blue, 0.1f, false);
        Debug.DrawRay(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), Color.blue, 0.1f, false);
        Debug.DrawRay(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), Color.blue, 0.1f, false);
        RaycastHit hitLower;
        RaycastHit hitLower45;
        RaycastHit hitLowerMinus45;
        RaycastHit downStairs;
        Vector3 stepPos = moveDirection * delta;

        if(inputHandler.moveAmount > 0)
        {
            if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.1f, ignoreGroundCheck)
                && (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitLower45, 0.1f, ignoreGroundCheck) 
                || Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitLowerMinus45, 0.1f, ignoreGroundCheck)))
            {
                Debug.Log("stepLower");
                RaycastHit hitUpper;
                RaycastHit hitUpper45;
                RaycastHit hitUpperMinus45;
                if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.12f, ignoreGroundCheck)
                    && (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitUpper45, 0.12f, ignoreGroundCheck)
                    || !Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitUpperMinus45, 0.12f, ignoreGroundCheck)))
                {
                    Debug.Log("step");
                    stepPos -= new Vector3(0f, -stepHeight, 0f);
                    rigidbody.position += stepPos;
                }
            }
            else if (Physics.Raycast(myTransform.position, -Vector3.up, out downStairs, groundDirectionRayDistance, ignoreGroundCheck))
            {
                targetPosition = myTransform.position;
                Vector3 tp = downStairs.point;
                targetPosition.y = tp.y;
                if (!playerManager.isInteracting)
                {
                    myTransform.position = targetPosition;
                }
            }
        }
    }
    #endregion
}
