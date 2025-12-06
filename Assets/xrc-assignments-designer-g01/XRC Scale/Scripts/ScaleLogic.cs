using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScaleLogic : MonoBehaviour
{
    public GameObject editObject;
    public bool isEditModeActive;
    public Action OnEnterEditMode;
    public Action OnExitEditMode;

    private Vector3 currentScale = Vector3.one;
    private bool wasKinematic;
    private bool wasGrabEnabled;

    public Vector3 scale
    {
        get => currentScale;
        set => currentScale = value;
    }

    public void EnterEditMode()
    {
        if (editObject == null)
        {
            return;
        }
        
        if (isEditModeActive)
        {
            return;
        }

        currentScale = editObject.transform.localScale;
        isEditModeActive = true;
        
        var grab = editObject.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            wasGrabEnabled = grab.enabled;
            grab.enabled = false; 
        }

        var rb = editObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        if (OnEnterEditMode != null) OnEnterEditMode.Invoke();
    }

    public void ExitEditMode()
    {
        if (!isEditModeActive)
        {
            return;
        }
        isEditModeActive = false;

        if (editObject != null)
        {
            var grab = editObject.GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.enabled = wasGrabEnabled;
            }

            var rb = editObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = wasKinematic;
            }
        }

        if (OnExitEditMode != null) OnExitEditMode.Invoke();
    }

    public void OnEditObjectChanging(GameObject newObject)
    {
        if (newObject == editObject)
        {
            return;
        }

        if (isEditModeActive)
        {
            ExitEditMode();
        }

        editObject = newObject;

        if (editObject != null)
        {
            currentScale = editObject.transform.localScale;
        }
    }

    public void SetScale(Vector3 newScale)
    {
        scale = newScale;
        if (editObject != null)
        {
            editObject.transform.localScale = scale;
        }
    }
}