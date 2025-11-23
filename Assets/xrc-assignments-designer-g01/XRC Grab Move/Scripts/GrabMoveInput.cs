using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabMoveInput : MonoBehaviour
{
    [SerializeField] 
    private InputActionProperty leftGrab;

    [SerializeField]
    private InputActionProperty rightGrab;
    
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    
    private GrabMoveLogicScaling scalingLogic;
    private GrabMoveLogicCoreMovement grabMoveLogic;
    
    private Vector3 leftControllerPosition;
    private Vector3 rightControllerPosition;
    
    private bool isActive = false;

    private void Awake()
    {
        grabMoveLogic = GetComponent<GrabMoveLogicCoreMovement>();
        scalingLogic = GetComponent<GrabMoveLogicScaling>();
    }

    private void OnEnable()
    {
        if (leftGrab.action != null)
        {
            leftGrab.action.Enable();
        }

        if (rightGrab.action != null)
        {
            rightGrab.action.Enable();
        }
    }

    void OnDisable()
    {
        if (leftGrab.action != null)
        {
            leftGrab.action.Disable();
        }

        if (rightGrab.action != null)
        {
            rightGrab.action.Disable();
        }
    }

    private void Update()
    {
        float leftValue  = leftGrab.action != null  ? leftGrab.action.ReadValue<float>()  : 0f;
        float rightValue = rightGrab.action != null ? rightGrab.action.ReadValue<float>() : 0f;

        bool leftPressed  = leftValue  > 0.1f;
        bool rightPressed = rightValue > 0.1f;

        bool bothPressed = leftPressed && rightPressed;

        if (bothPressed)
        {
            if (grabMoveLogic != null && !grabMoveLogic.IsRunning)
            {
                grabMoveLogic.Begin();
            }
            
            if (scalingLogic != null && !scalingLogic.IsRunning)
            {
                scalingLogic.Begin();
            }
        }
        else
        {
            if (grabMoveLogic != null && grabMoveLogic.IsRunning)
            {
                grabMoveLogic.End();
            }

            if (scalingLogic != null && scalingLogic.IsRunning)
            {
                scalingLogic.End();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftControllerPosition, 0.05f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rightControllerPosition, 0.05f);

        
        if (isActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftControllerPosition, rightControllerPosition);
        }
    }
}