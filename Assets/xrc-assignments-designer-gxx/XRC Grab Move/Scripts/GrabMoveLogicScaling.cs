using UnityEngine;

public class GrabMoveLogicScaling : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    [SerializeField] private GrabMoveFeedback feedback;

    [Header("State")]
    private bool isRunning = false;
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    
    [Header("Scale Settings")]
    [SerializeField] private float currentScale = 1.0f;
    [SerializeField] private float minScale = 0.01f;
    [SerializeField] private float maxScale = 10.0f;
    private float prevScaleValue;
    
    public bool IsRunning => isRunning;
    
    private void Update()
    {
        if (!isRunning) return;
        if (playerTransform == null || leftController == null || rightController == null)
            return;
        
        Vector3 leftPos = leftController.position;
        Vector3 rightPos = rightController.position;
        
        Vector3 pivot = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(leftPos, rightPos);
        
        float newScale = GrabMoveCalculationsScaling.CalculateProportionalScale(
            leftPos, rightPos, prevLeftPos, prevRightPos, 
            currentScale, minScale, maxScale
        );
        if (Mathf.Abs(newScale - currentScale) < 0.001f)
        {
            prevLeftPos = leftPos;
            prevRightPos = rightPos;
            return;
        }
        Vector3 offset = GrabMoveCalculationsScaling.CalculateScaleOffset(
            pivot, playerTransform.position, newScale, currentScale
        );
        
        playerTransform.localScale = Vector3.one * newScale;
        playerTransform.position += offset;
        
        if (feedback != null)
        {
            feedback.UpdatePivotVisual(pivot);
            feedback.UpdateLineVisual(leftPos, rightPos);
            feedback.UpdateScaleText(GetWorldScalePercent());
        }

        prevScaleValue = currentScale;
        currentScale = newScale;
        prevLeftPos = leftPos;
        prevRightPos = rightPos;
    }
    
    public void Begin()
    {
        Debug.Log("SCALING BEGIN CALLED"); 
        if (leftController == null || rightController == null)
        {
            Debug.LogWarning("GrabMoveLogicScaling: Controllers not assigned!");
            return;
        }
        
        isRunning = true;
        
        prevLeftPos = leftController.position;
        prevRightPos = rightController.position;
        prevScaleValue = currentScale;
        
        if (feedback != null)
        {
            feedback.InitializeFeedbackInstances();
            feedback.SetFeedbackActive(true);
        }
        
        Debug.Log("Scaling BEGIN");
    }
    
    public void End()
    {
        isRunning = false;
        if (feedback != null)
        {
            feedback.SetFeedbackActive(false);
        }
        
        Debug.Log("Scaling END");
    }
    
    public float GetWorldScalePercent()
    {
        return currentScale * 100f;
    }
    
    public float GetCurrentScale()
    {
        return currentScale;
    }
}
