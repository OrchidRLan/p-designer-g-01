using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabMoveInput : MonoBehaviour
{
    [SerializeField]
    private InputActionProperty leftGrab;

    [SerializeField]
    private InputActionProperty rightGrab;

    private GrabMoveLogicCoreMovement grabMoveLogic;

    private void Awake()
    {
        grabMoveLogic = GetComponent<GrabMoveLogicCoreMovement>();
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
        float leftValue = 0f; 
        if (leftGrab.action != null)
        {
            leftValue = leftGrab.action.ReadValue<float>();
        }
        
        float rightValue = 0f;
        if (rightGrab.action != null)
        {
            rightValue = rightGrab.action.ReadValue<float>();
        }
        bool leftPressed = leftValue > 0.1f;
        bool rightPressed = rightValue > 0.1f;

        if (leftPressed && rightPressed)
        {
            if (!grabMoveLogic.IsRunning)
            {
                grabMoveLogic.Begin();
            }
        }
        else
        {
            if (grabMoveLogic.IsRunning)
            {
                grabMoveLogic.End();
            }
        }
    }
}