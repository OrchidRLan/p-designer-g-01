using UnityEngine;
using TMPro;

public class GrabMoveFeedback : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject pivotPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject textPrefab;
    
    private GameObject pivotInstance;
    private GameObject lineInstance;
    private GameObject textInstance;
    
    private LineRenderer lineRenderer;
    private TMPro.TextMeshPro textComponent;
    
    private const float TEXT_OFFSET_Y = 0.06f;
    private const float FIXED_PIVOT_SIZE = 0.02f;
    private const float FIXED_TEXT_SCALE = 0.01f;
    private const float FIXED_LINE_WIDTH = 0.02f;
    
    private bool isInitialized = false;
    
    public void InitializeFeedbackInstances()
    {
        if (isInitialized)
        {
            return;
        }
        
        if (pivotPrefab == null || linePrefab == null || textPrefab == null)
        {
            return;
        }
        
        pivotInstance = Instantiate(pivotPrefab);
        lineInstance = Instantiate(linePrefab);
        textInstance = Instantiate(textPrefab);
        
        pivotInstance.transform.SetParent(null);
        lineInstance.transform.SetParent(null);
        textInstance.transform.SetParent(null);
        
        pivotInstance.transform.localScale = Vector3.one * FIXED_PIVOT_SIZE;
        textInstance.transform.localScale = Vector3.one * FIXED_TEXT_SCALE;
        
        lineRenderer = lineInstance.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.widthMultiplier = FIXED_LINE_WIDTH;
        }
        
        textComponent = textInstance.GetComponentInChildren<TMPro.TextMeshPro>();

        pivotInstance.SetActive(false);
        lineInstance.SetActive(false);
        textInstance.SetActive(false);
        
        isInitialized = true;
    }

    public void UpdatePivotVisual(Vector3 pivotPosition)
    {
        if (pivotInstance != null)
        {
            pivotInstance.transform.position = pivotPosition;
        }
    }

    public void UpdateLineVisual(Vector3 leftPos, Vector3 rightPos)
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, leftPos);
            lineRenderer.SetPosition(1, rightPos);
        }
    }

    public void UpdateScaleText(float worldScalePercent)
    {
        if (textInstance == null || textComponent == null || lineRenderer == null)
            return;
        
        textComponent.text = worldScalePercent.ToString("F0") + "%";
        
        Vector3 leftPos = lineRenderer.GetPosition(0);
        Vector3 rightPos = lineRenderer.GetPosition(1);
        
        Vector3 midpoint = (leftPos + rightPos) * 0.5f;
        
        textInstance.transform.position = midpoint + Vector3.up * TEXT_OFFSET_Y;
        
        Vector3 lineDirection = (rightPos - leftPos).normalized;
        textInstance.transform.right = lineDirection;
    }

    public void SetFeedbackActive(bool isActive)
    {
        if (!isInitialized)
        {
            return;
        }
        
        if (pivotInstance != null)
            pivotInstance.SetActive(isActive);
        
        if (lineInstance != null)
            lineInstance.SetActive(isActive);
            
        if (textInstance != null)
            textInstance.SetActive(isActive);
    }
    
    private void OnDestroy()
    {
        if (pivotInstance != null) Destroy(pivotInstance);
        if (lineInstance != null) Destroy(lineInstance);
        if (textInstance != null) Destroy(textInstance);
    }
}