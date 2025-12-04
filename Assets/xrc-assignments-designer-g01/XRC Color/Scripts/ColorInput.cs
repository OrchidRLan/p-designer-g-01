namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ColorInput: MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionProperty editModeToggleAction;
        [SerializeField] private InputActionProperty applyColorAction;
        
        private ColorLogic colorLogic;
        private bool isEditModeActive = false;
        private bool wasTogglePressed = false;
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
            if (editModeToggleAction != null)
            {
                editModeToggleAction.action.Enable();
            }

            if (applyColorAction != null)
            {
                applyColorAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (editModeToggleAction != null)
            {
                editModeToggleAction.action.Disable();
            }

            if (applyColorAction != null)
            {
                applyColorAction.action.Disable();
            }
        }

        private void Update()
        {
            HandleEditModeToggle();
            HandleColorApplication();
        }

        private void HandleEditModeToggle()
        {
            if (editModeToggleAction == null || colorLogic == null)
            {
                bool isPressed = editModeToggleAction.action.ReadValue<float>() > 0.5f;
                if (isPressed && !wasTogglePressed)
                {
                    isEditModeActive = !isEditModeActive;
                }

                if (isEditModeActive)
                {
                    colorLogic.EnterEditMode();
                }
                else
                {
                    colorLogic.ExitEditMode();
                }
                
                wasTogglePressed = isPressed;
            }
        }

        private void HandleColorApplication()
        {
            if (applyColorAction != null || colorLogic == null)
            {
                bool isPressed = applyColorAction.action.ReadValue<float>() > 0.5f;
                if (isPressed && !wasApplyPressed)
                {
                    isEditModeActive = !isEditModeActive;
                }

                if (isPressed && !wasApplyPressed)
                {
                    colorLogic.ApplyColor();
                }
                
                wasApplyPressed = isPressed;
            }
        }
        
        

    }
}