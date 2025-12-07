using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MeshInput : MonoBehaviour
{
    [SerializeField] private MeshLogic meshLogic;
    
    [Header("Input Actions")]
    [SerializeField] 
    private InputActionProperty enterEditModeAction;
    [SerializeField] 
    private InputActionProperty exitEditModeAction;

    private bool isHolding = false;

    private void OnEnable()
    {
        if (enterEditModeAction.action != null)
        {
            enterEditModeAction.action.performed += OnEnterEditModePerformed;
            enterEditModeAction.action.Enable();
        }

        if (exitEditModeAction.action != null)
        {
            exitEditModeAction.action.performed += OnExitEditModePerformed;
            exitEditModeAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (enterEditModeAction.action != null)
        {
            enterEditModeAction.action.performed -= OnEnterEditModePerformed;
            enterEditModeAction.action.Disable();
        }

        if (exitEditModeAction.action != null)
        {
            exitEditModeAction.action.performed -= OnExitEditModePerformed;
            exitEditModeAction.action.Disable();
        }
    }

    private void OnEnterEditModePerformed(InputAction.CallbackContext ctx)
    {
        if (meshLogic == null) return;
        
        if (!meshLogic.IsInEditMode && meshLogic.HasTarget)
        {
            Debug.Log("[XRC Mesh] Enter Action Performed");
            meshLogic.BeginEditMode();
        }
    }
    private void OnExitEditModePerformed(InputAction.CallbackContext ctx)
    {
        if (meshLogic == null) return;
        if (meshLogic.IsInEditMode)
        {
            Debug.Log("[XRC Mesh] Exit Action Performed");
            meshLogic.EndEditMode();
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (meshLogic.IsInEditMode)
        {
            return;
        }

        if (args.interactableObject != null)
        {
            if (args.interactableObject.transform.IsChildOf(this.transform))
            {
                return;
            }
        }

        isHolding = true;

        if (args.interactableObject != null)
        {
            GameObject grabbedObject = args.interactableObject.transform.gameObject;
            if (meshLogic != null)
            {
                meshLogic.OnEditObjectChanging(grabbedObject);
            }
        }
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (meshLogic.IsInEditMode)
        {
            return;
        }

        if (args.interactableObject != null)
        {
            if (args.interactableObject.transform.IsChildOf(this.transform))
            {
                return;
            }
        }

        isHolding = false;

        if (meshLogic != null)
        {
            meshLogic.OnEditObjectChanging(null);
        }
    }
}