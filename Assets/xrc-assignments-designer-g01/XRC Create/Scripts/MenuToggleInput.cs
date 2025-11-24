using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggleInput : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference toggleAction;

    [Header("Menu UI")]
    public GameObject menuUI;

    private void Start()
    {
        toggleAction.action.Enable();
    }

    private void OnEnable()
    {
        toggleAction.action.performed += OnToggle;
        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= OnToggle;
        toggleAction.action.Disable();
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        menuUI.SetActive(!menuUI.activeSelf);
    }
    
    
}