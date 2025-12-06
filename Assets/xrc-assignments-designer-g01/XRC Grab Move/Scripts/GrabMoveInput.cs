using UnityEngine;
using UnityEngine.InputSystem;

public class GrabMoveInput : MonoBehaviour
{
    [SerializeField] private InputActionProperty leftGrab;
    [SerializeField] private InputActionProperty rightGrab;
    
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    
    private GrabMoveLogic grabMoveLogic;
    
    private void Awake()
    {
        grabMoveLogic = GetComponent<GrabMoveLogic>();
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

    private void OnDisable()
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

        if (grabMoveLogic == null) return;

        if (bothPressed)
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

    private void OnDrawGizmos()
    {
        if (leftController == null || rightController == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftController.position, 0.05f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rightController.position, 0.05f);

        if (grabMoveLogic != null && grabMoveLogic.IsRunning)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftController.position, rightController.position);
        }
    }
}