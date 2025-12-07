using UnityEngine;

public class MeshIconController : MonoBehaviour
{
    [Header("Icon References")]
    [SerializeField] private GameObject enterEditIcon; 
    [SerializeField] private GameObject confirmIcon;  
    
    [Header("State Control Reference")]
    [SerializeField] private MeshLogic meshLogic;

    private void Start()
    {
        UpdateIconVisibility();
    }

    private void Update()
    {
        UpdateIconVisibility();
    }

    private void UpdateIconVisibility()
    {
        if (meshLogic == null) return;
        
        bool hasEditObject = meshLogic.HasTarget;
        bool inEditMode = meshLogic.IsInEditMode;
        bool isGrabbing = meshLogic.IsGrabbing;
        
        if (enterEditIcon != null)
        {
            enterEditIcon.SetActive(hasEditObject && isGrabbing && !inEditMode);
        }

        if (confirmIcon != null)
        {
            confirmIcon.SetActive(inEditMode);
        }
    }
}