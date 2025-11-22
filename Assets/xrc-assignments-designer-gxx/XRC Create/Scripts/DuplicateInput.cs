using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuplicateInput : MonoBehaviour
{
    [Header("Input")] public InputActionProperty duplicateAction;
    public InputActionProperty deleteAction;
    public event Action OnDuplicate;
    public event Action OnDelete;

    private void OnEnable()
    {
        if (duplicateAction.action != null)
        {
            duplicateAction.action.performed += HandleDuplicate;
            duplicateAction.action.Enable();
        }

        if (deleteAction.action != null)
        {
            deleteAction.action.performed += HandleDelete;
            deleteAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (duplicateAction.action != null)
        {
            duplicateAction.action.performed -= HandleDuplicate;
            duplicateAction.action.Disable();
        }

        if (deleteAction.action != null)
        {
            deleteAction.action.performed -= HandleDelete;
            deleteAction.action.Disable();
        }
    }
    void HandleDuplicate(InputAction.CallbackContext _) => OnDuplicate?.Invoke();
    void HandleDelete(InputAction.CallbackContext _)    => OnDelete?.Invoke();
}