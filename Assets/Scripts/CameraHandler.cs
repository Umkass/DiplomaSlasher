using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : Singleton<CameraHandler>
{

    public Transform targetTransform;
    public Transform cameraTransform;
    public Transform cameraPivotTransform;
    private Transform myTransform;
    private Vector3 cameraTransformPosition;
    private LayerMask ignoreLayer;
    private Vector3 cameraFollowVelocity = Vector3.zero;

    public float lookSpeed = 0.1f;
    public float followSpeed = 0.1f;
    public float pivotSpeed = 0.03f;
    private float targetPositionZ;
    private float defaultPositionZ;
    private float lookAngle;
    private float pivotAnge;
    public float minimumPivot = -35f;
    public float maximumPivot = 35f;

    public float cameraSphereRadius = 0.2f;
    public float cameraCollisionOffset = 0.2f;
    public float minimumCollisionOffset = 0.2f;

    protected override void Awake()
    {
        myTransform = transform;
        defaultPositionZ = cameraTransform.localPosition.z;
        ignoreLayer = ~(1 << 8 | 1 << 9 | 1 << 10);
    }

    public void FollowTarget(float delta)
    {
        Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position,targetTransform.position,ref cameraFollowVelocity, delta / followSpeed);
        myTransform.position = targetPosition;

        CameraCollisions(delta);
    }

    public void CameraRotation(float delta,float mouseInputX, float mouseInputY)
    {
        lookAngle += (mouseInputX * lookSpeed) / delta;
        pivotAnge -= (mouseInputY * pivotSpeed) / delta;
        pivotAnge = Mathf.Clamp(pivotAnge,minimumPivot,maximumPivot);

        Vector3 rotation = Vector3.zero;
        rotation.y = lookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        myTransform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAnge;

        targetRotation = Quaternion.Euler(rotation);
        cameraPivotTransform.localRotation = targetRotation;
    }

    public void CameraCollisions(float delta)
    {
        targetPositionZ = defaultPositionZ;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
        direction.Normalize();
        if (Physics.SphereCast(cameraPivotTransform.position,cameraSphereRadius,direction,out hit, Mathf.Abs(targetPositionZ),ignoreLayer))
        {
            float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetPositionZ = -(dis - cameraCollisionOffset);
        }
        if (Mathf.Abs(targetPositionZ) < minimumCollisionOffset)
        {
            targetPositionZ = -minimumCollisionOffset;
        }

        cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPositionZ, delta / 0.2f);
        cameraTransform.localPosition = cameraTransformPosition;
    }

}
