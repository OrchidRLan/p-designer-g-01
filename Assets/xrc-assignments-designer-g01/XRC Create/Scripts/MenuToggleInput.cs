using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MenuToggleInput : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference toggleAction;

    [Header("Menu UI")]
    public GameObject menuUI;
    public GameObject menuIcon;

    [SerializeField]
    private XRBaseInteractor handInteractor;

    private void Start()
    {
        toggleAction.action.Enable();
    }

    private void OnEnable()
    {
        toggleAction.action.performed += OnToggle;
        toggleAction.action.Enable();

        if (handInteractor != null)
        {
            handInteractor.selectEntered.AddListener(OnSelectEntered);
            handInteractor.selectExited.AddListener(OnSelectExited);
        }
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= OnToggle;
        toggleAction.action.Disable();

        if (handInteractor != null)
        {
            handInteractor.selectEntered.RemoveListener(OnSelectEntered);
            handInteractor.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (handInteractor != null && handInteractor.hasSelection)
        {
            return;
        }

        menuUI.SetActive(!menuUI.activeSelf);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (menuIcon != null)
        {
            menuIcon.SetActive(false);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (menuIcon != null)
        {
            menuIcon.SetActive(true);
        }
    }
}