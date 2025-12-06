using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class ScaleInput : MonoBehaviour
{
    [SerializeField]
    private InputActionProperty toggleEditModeInput;

    [SerializeField]
    private InputActionProperty enterEditModeInput;

    [SerializeField]
    private InputActionProperty exitEditModeInput;

    public UnityEvent OnEditAvailable;
    public UnityEvent OnEditUnavailable;
    public UnityEvent OnEditModeEntered;
    public UnityEvent OnEditModeExited;
    private ScaleLogic scaleLogic;
    private bool isHolding = false;
    private void Awake()
    {
        scaleLogic = GetComponent<ScaleLogic>();
    }
    private void OnEnable()
    {
        if (toggleEditModeInput.action != null)
        {
            toggleEditModeInput.action.performed += HandleToggleEdit;
            toggleEditModeInput.action.Enable();
        }

        if (enterEditModeInput.action != null)
        {
            enterEditModeInput.action.performed += HandleEnterEdit;
            enterEditModeInput.action.Enable();
        }

        if (exitEditModeInput.action != null)
        {
            exitEditModeInput.action.performed += HandleExitEdit;
            exitEditModeInput.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleEditModeInput.action != null)
        {
            toggleEditModeInput.action.performed -= HandleToggleEdit;
            toggleEditModeInput.action.Disable();
        }

        if (enterEditModeInput.action != null)
        {
            enterEditModeInput.action.performed -= HandleEnterEdit;
            enterEditModeInput.action.Disable();
        }

        if (exitEditModeInput.action != null)
        {
            exitEditModeInput.action.performed -= HandleExitEdit;
            exitEditModeInput.action.Disable();
        }
    }

    private void HandleToggleEdit(InputAction.CallbackContext context)
    {
        if (scaleLogic == null || scaleLogic.editObject == null)
        {
            return;
        }

        if (scaleLogic.isEditModeActive)
        {
            scaleLogic.ExitEditMode();
            
            if (OnEditModeExited != null)
            {
                OnEditModeExited.Invoke();
            }

            if (!isHolding)
            {
                scaleLogic.OnEditObjectChanging(null);
                if (OnEditUnavailable != null)
                {
                    OnEditUnavailable.Invoke();
                }
            }
        }
        else
        {
            scaleLogic.EnterEditMode();
            if (OnEditModeEntered != null)
            {
                OnEditModeEntered.Invoke();
            }
        }
    }

    private void HandleEnterEdit(InputAction.CallbackContext context)
    {
        if (scaleLogic == null || scaleLogic.editObject == null)
        {
            return;
        }

        if (!scaleLogic.isEditModeActive)
        {
            scaleLogic.EnterEditMode();
            if (OnEditModeEntered != null)
            {
                OnEditModeEntered.Invoke();
            }
        }
    }

    private void HandleExitEdit(InputAction.CallbackContext context)
    {
        if (scaleLogic.isEditModeActive)
        {
            scaleLogic.ExitEditMode();
            if (OnEditModeExited != null)
            {
                OnEditModeExited.Invoke();
            }
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (scaleLogic.isEditModeActive)
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
                
            if (scaleLogic != null) 
            {
                scaleLogic.OnEditObjectChanging(grabbedObject);
                if (!scaleLogic.isEditModeActive)
                {
                    if (OnEditAvailable != null)
                    {
                        OnEditAvailable.Invoke();
                    }
                }
            }
        }
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (scaleLogic.isEditModeActive)
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

        if (scaleLogic != null)
        {
            if (!scaleLogic.isEditModeActive)
            {
                scaleLogic.OnEditObjectChanging(null);
                if (OnEditUnavailable != null)
                {
                    OnEditUnavailable.Invoke();
                }
            }
        }
    }
}