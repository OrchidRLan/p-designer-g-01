using UnityEngine;
using TMPro;

public class GrabMoveFeedback : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject pivotPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject textPrefab;
    
    [Header("Scale Compensation")]
    [SerializeField] private ScaleCompensation scaleCompensation;
    
    private GameObject pivotInstance;
    private GameObject lineInstance;
    private GameObject textInstance;
    
    private LineRenderer lineRenderer;
    private TextMeshPro textComponent;
    
    [Header("Settings")]
    [SerializeField] private float textOffsetY = 0.2f;
    [SerializeField] private float fixedPivotSize = 0.02f;
    [SerializeField] private float fixedTextScale = 0.01f;
    [SerializeField] private float fixedLineWidth = 0.002f;
    
    private bool isInitialized = false;
    
    public void InitializeFeedbackInstances()
    {
        if (isInitialized)
        {
            return;
        }
        
        if (pivotPrefab == null || linePrefab == null || textPrefab == null)
        {
            Debug.LogError("GrabMoveFeedback: Prefabs not assigned!");
            return;
        }
        
        if (scaleCompensation == null)
        {
            Debug.LogError("GrabMoveFeedback: ScaleCompensation not assigned!");
            return;
        }
        
        pivotInstance = Instantiate(pivotPrefab);
        lineInstance = Instantiate(linePrefab);
        textInstance = Instantiate(textPrefab);
        
        pivotInstance.transform.localScale = Vector3.one * fixedPivotSize;
        textInstance.transform.localScale = Vector3.one * fixedTextScale;
        
        lineRenderer = lineInstance.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.widthMultiplier = fixedLineWidth;
        }
        else
        {
            Debug.LogError("GrabMoveFeedback: LineRenderer not found on Line prefab!");
        }
        
        textComponent = textInstance.GetComponentInChildren<TextMeshPro>();
        if (textComponent == null)
        {
            Debug.LogError("GrabMoveFeedback: TextMeshPro not found on Text prefab!");
        }

        scaleCompensation.AddCompensatedObject(
            pivotInstance.transform, 
            ScaleCompensation.ObjectType.Transform3D, 
            pivotInstance.transform.localScale
        );
        
        scaleCompensation.AddCompensatedObject(
            textInstance.transform, 
            ScaleCompensation.ObjectType.Transform3D, 
            textInstance.transform.localScale
        );
        
        scaleCompensation.AddCompensatedObject(
            lineInstance.transform, 
            ScaleCompensation.ObjectType.LineRenderer, 
            new Vector3(fixedLineWidth, 0, 0)
        );
        
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
            // 缩放补偿由ScaleCompensation系统自动处理
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

    public void UpdateScaleText(float worldScalePercent, Vector3 pivotPos)
    {
        if (textInstance == null || textComponent == null || scaleCompensation == null)
            return;
        
        textComponent.text = worldScalePercent.ToString("F0") + "%";
        
        float compensationFactor = scaleCompensation.GetCompensationFactor();
        float adjustedOffset = textOffsetY * compensationFactor;

        textInstance.transform.position = pivotPos + Vector3.up * adjustedOffset;
        
        var cam = Camera.main;
        if (cam != null)
        {
            textInstance.transform.LookAt(cam.transform);
            textInstance.transform.Rotate(0, 180, 0);
        }
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
        if (scaleCompensation != null)
        {
            if (pivotInstance != null)
                scaleCompensation.RemoveCompensatedObject(pivotInstance.transform);
            if (lineInstance != null)
                scaleCompensation.RemoveCompensatedObject(lineInstance.transform);
            if (textInstance != null)
                scaleCompensation.RemoveCompensatedObject(textInstance.transform);
        }
        
        if (pivotInstance != null) Destroy(pivotInstance);
        if (lineInstance != null) Destroy(lineInstance);
        if (textInstance != null) Destroy(textInstance);
    }
}