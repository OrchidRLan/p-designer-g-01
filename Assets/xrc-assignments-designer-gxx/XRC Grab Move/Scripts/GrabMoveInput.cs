using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
/// <summary>
/// 临时占位符 - 等Gino完成后替换
/// </summary>
public class GrabMoveInput : MonoBehaviour
{
    private GrabMoveLogicCoreMovement coreLogic;
    private GrabMoveLogicScaling scalingLogic;
    
    [Header("Controller References")]
    [SerializeField] private XRNode leftControllerNode = XRNode.LeftHand;
    [SerializeField] private XRNode rightControllerNode = XRNode.RightHand;
    

    [Header("Input Settings")]
    [SerializeField] private InputHelpers.Button activationButton = InputHelpers.Button.Grip;
    [SerializeField] private bool requireBothButtons = true;
    
    private bool isActive = false;
    private bool leftButtonPressed = false;
    private bool rightButtonPressed = false;
    
    private Vector3 leftControllerPosition;
    private Vector3 rightControllerPosition;
    
    
    void Update()
    {
        UpdateControllerPositions();

        UpdateButtonStates();
        
        bool shouldBeActive = requireBothButtons 
            ? (leftButtonPressed && rightButtonPressed)
            : (leftButtonPressed || rightButtonPressed);
        
        if (shouldBeActive && !isActive)
        {
            HandleGrabMoveActivation();
        }
        else if (!shouldBeActive && isActive)
        {
            HandleGrabMoveDeactivation();
        }
        
        if (isActive)
        {
            UpdateGrabMove();
        }
    }
    

    
    private void UpdateControllerPositions()
    {

        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftControllerNode);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightControllerNode);
        
        if (leftDevice.isValid && leftDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos))
        {
            leftControllerPosition = leftPos;
        }
        
        if (rightDevice.isValid && rightDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos))
        {
            rightControllerPosition = rightPos;
        }
        
        
    }
    

    private void UpdateButtonStates()
    {
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftControllerNode);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightControllerNode);
        
        if (leftDevice.isValid)
        {
            leftDevice.IsPressed(activationButton, out leftButtonPressed);
        }
        

        if (rightDevice.isValid)
        {
            rightDevice.IsPressed(activationButton, out rightButtonPressed);
        }
    }
    

    private void HandleGrabMoveActivation()
    {
        isActive = true;
        
        Debug.Log("Grab Move: BEGIN");
        
        if (coreLogic != null)
        {
            coreLogic.Begin(leftControllerPosition, rightControllerPosition);
        }
        
        if (scalingLogic != null)
        {
            scalingLogic.Begin(leftControllerPosition, rightControllerPosition);
        }
    }
    
    private void HandleGrabMoveDeactivation()
    {
        isActive = false;
        
        Debug.Log("Grab Move: END");
        
        if (coreLogic != null)
        {
            coreLogic.End();
        }
        
        if (scalingLogic != null)
        {
            scalingLogic.End();
        }
    }
    
    private void UpdateGrabMove()
    {
        if (coreLogic != null)
        {
            coreLogic.UpdateMovement(leftControllerPosition, rightControllerPosition);
        }
        
        if (scalingLogic != null)
        {
            scalingLogic.UpdateScaling(leftControllerPosition, rightControllerPosition);
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