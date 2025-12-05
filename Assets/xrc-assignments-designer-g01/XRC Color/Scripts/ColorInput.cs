using UnityEngine;
using UnityEngine.InputSystem;

public class ColorInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionProperty enterEditModeAction; 
    [SerializeField] private InputActionProperty confirmAction;     
    [SerializeField] private InputActionProperty applyColorAction;  
    
    [Header("References")]
    [SerializeField] private ColorLogic colorLogic;
    
    private bool wasEnterPressed = false;
    private bool wasConfirmPressed = false;
    private bool wasApplyPressed = false;

    private void Awake()
    {
        if (colorLogic == null)
        {
            colorLogic = GetComponent<ColorLogic>();
        }
    }

    private void OnEnable()
    {
        enterEditModeAction.action?.Enable();
        confirmAction.action?.Enable();
        applyColorAction.action?.Enable();
    }

    private void OnDisable()
    {
        enterEditModeAction.action?.Disable();
        confirmAction.action?.Disable();
        applyColorAction.action?.Disable();
    }

    private void Update()
    {
        HandleEnterEditMode();
        HandleConfirm();
        HandleColorApplication();
    }

    private void HandleEnterEditMode()
    {
        if (enterEditModeAction.action == null || colorLogic == null) return;
        
        bool isPressed = enterEditModeAction.action.ReadValue<float>() > 0.5f;
        
        if (isPressed && !wasEnterPressed && !colorLogic.IsEditMode)
        {
            colorLogic.EnterEditMode();
        }
        
        wasEnterPressed = isPressed;
    }

    private void HandleConfirm()
    {
        if (confirmAction.action == null || colorLogic == null) return;
        
        bool isPressed = confirmAction.action.ReadValue<float>() > 0.5f;
        
        if (isPressed && !wasConfirmPressed)
        {
            if (colorLogic.IsEditMode)
            {
                colorLogic.ExitEditMode();
            }
        }
        
        wasConfirmPressed = isPressed;
    }

    private void HandleColorApplication()
    {
        if (applyColorAction.action == null || colorLogic == null) return;
        
        if (!colorLogic.IsEditMode) return;
        
        bool isPressed = applyColorAction.action.ReadValue<float>() > 0.5f;
        
        if (isPressed && !wasApplyPressed)
        {
            colorLogic.ApplyColor();
        }
        
        wasApplyPressed = isPressed;
    }
}