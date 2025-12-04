using UnityEngine;
using UnityEngine.InputSystem;

public class MeshInput : MonoBehaviour
{
    [SerializeField] private MeshLogic meshLogic;
    [SerializeField] private InputActionProperty editModeAction;

    private void OnEnable()
    {
        if (editModeAction.action == null) return;

        editModeAction.action.performed += OnEditModePerformed;
        editModeAction.action.Enable();
    }

    private void OnDisable()
    {
        if (editModeAction.action == null) return;

        editModeAction.action.performed -= OnEditModePerformed;
        editModeAction.action.Disable();
    }

    private void OnEditModePerformed(InputAction.CallbackContext ctx)
    {
        if (meshLogic == null) return;

        if (meshLogic.IsInEditMode)
            meshLogic.EndEditMode();
        else
            meshLogic.BeginEditMode();
    }
}