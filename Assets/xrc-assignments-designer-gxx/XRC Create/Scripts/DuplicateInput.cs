using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuplicateInput : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty duplicateAction;

    public event Action OnDuplicate; 

    private void OnEnable()
    {
        if (duplicateAction.action == null) return;
        duplicateAction.action.performed += HandlePerformed;
        duplicateAction.action.Enable();
    }

    private void OnDisable()
    {
        if (duplicateAction.action == null) return;
        duplicateAction.action.performed -= HandlePerformed;
        duplicateAction.action.Disable();
    }

    private void HandlePerformed(InputAction.CallbackContext _)
    {
        OnDuplicate?.Invoke();
    }
}