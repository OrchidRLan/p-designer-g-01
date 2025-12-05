using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ColorLogic : MonoBehaviour
{
    [Header("Edit Target")]
    [SerializeField] private GameObject editObject;
    [SerializeField] private Color currentColor = Color.white;
    
    [Header("References")]
    [SerializeField] private ColorWidget colorWidget;
    [SerializeField] private ColorFeedback colorFeedback;
    
    [Header("Grab Detection")]
    [SerializeField] private XRDirectInteractor rightHandInteractor;
    [SerializeField] private XRDirectInteractor leftHandInteractor;
    
    private Renderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;
    private bool isEditMode = false;
    private bool isRightHandGrabbingEditObject = false;
    
    public bool IsEditMode => isEditMode;
    public bool IsRightHandGrabbingEditObject => isRightHandGrabbingEditObject;
    
    public Color Color
    {
        get => currentColor;
        set => currentColor = value;
    }
    
    public GameObject EditObject
    {
        get => editObject;
        set
        {
            editObject = value;
            UpdateTargetRenderer();
        }
    }

    private void Awake()
    {
        if (colorWidget == null)
        {
            colorWidget = GetComponent<ColorWidget>();
        }
        
        if (colorFeedback == null)
        {
            colorFeedback = GetComponent<ColorFeedback>();
        }
        
        propertyBlock = new MaterialPropertyBlock();
        UpdateTargetRenderer();
    }

    private void OnEnable()
    {
        if (rightHandInteractor != null)
        {
            rightHandInteractor.selectEntered.AddListener(OnObjectGrabbed);
            rightHandInteractor.selectExited.AddListener(OnObjectReleased);
        }

        if (leftHandInteractor != null)
        {
            leftHandInteractor.selectEntered.AddListener(OnObjectGrabbed);
            leftHandInteractor.selectExited.AddListener(OnObjectReleased);
        }
    }

    private void OnDisable()
    {
        if (rightHandInteractor != null)
        {
            rightHandInteractor.selectEntered.RemoveListener(OnObjectGrabbed);
            rightHandInteractor.selectExited.RemoveListener(OnObjectReleased);
        }

        if (leftHandInteractor != null)
        {
            leftHandInteractor.selectEntered.RemoveListener(OnObjectGrabbed);
            leftHandInteractor.selectExited.RemoveListener(OnObjectReleased);
        }
    }

    private void OnObjectGrabbed(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;
        bool isRightHand = args.interactorObject == rightHandInteractor;
        
        if (isRightHand && grabbedObject.GetComponent<Renderer>() != null)
        {
            EditObject = grabbedObject;
            isRightHandGrabbingEditObject = true;
        }
    }

    private void OnObjectReleased(SelectExitEventArgs args)
    {
        bool isRightHand = args.interactorObject == rightHandInteractor;
        GameObject releasedObject = args.interactableObject.transform.gameObject;
        
        if (isRightHand && editObject != null && releasedObject == editObject)
        {
            isRightHandGrabbingEditObject = false;
        }
    }

    public void EnterEditMode()
    {
        if (editObject == null || targetRenderer == null)
        {
            return;
        }
        
        isEditMode = true;
        colorWidget.Show();
    }
    
    public void ExitEditMode()
    {
        isEditMode = false;
        colorWidget?.Hide();
        EditObject = null;
        isRightHandGrabbingEditObject = false;
    }

    public void ApplyColor()
    {
        if (editObject == null)
        {
            return;
        }
        
        if (targetRenderer == null)
        {
            return;
        }
        
        if (colorWidget == null)
        {
            return;
        }
        
        currentColor = colorWidget.GetCurrentColor();
        
        propertyBlock.SetColor("_Color", currentColor);
        propertyBlock.SetColor("_BaseColor", currentColor);
        propertyBlock.SetColor("_MainColor", currentColor);
        
        targetRenderer.SetPropertyBlock(propertyBlock);
        
        colorFeedback?.PlayApplySound();
    }

    private void UpdateTargetRenderer()
    {
        targetRenderer = editObject != null ? editObject.GetComponent<Renderer>() : null;
    }
}