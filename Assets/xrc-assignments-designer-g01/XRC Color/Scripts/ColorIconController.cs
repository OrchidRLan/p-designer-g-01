using UnityEngine;
public class ColorIconController : MonoBehaviour
{
    [Header("Icon References")]
    [SerializeField] private GameObject enterEditIcon; 
    [SerializeField] private GameObject confirmIcon;  
    
    [Header("State Control Reference")]
    [SerializeField] private ColorLogic colorLogic;

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
        bool hasEditObject = colorLogic.EditObject != null;
        bool inEditMode = colorLogic.IsEditMode;
        bool isGrabbing = colorLogic.IsRightHandGrabbingEditObject;
        
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