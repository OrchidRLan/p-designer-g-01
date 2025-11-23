using UnityEngine;
using TMPro;

public class GrabMoveFeedback : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject pivotPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform playerTransform;
    
    private GameObject pivotInstance;
    private GameObject lineInstance;
    private GameObject textInstance;
    
    private LineRenderer lineRenderer;
    private TMPro.TextMeshPro textComponent;

    
    [Header("Settings")]
    [SerializeField] private float textOffsetY = 0.2f;
    [SerializeField] private float fixedPivotSize = 0.1f;
    [SerializeField] private float fixedTextScale = 0.01f;
    
    private bool isInitialized = false;
    
    public void InitializeFeedbackInstances()
    {
        if (isInitialized)
        {
            Debug.LogWarning("Feedback already initialized!");
            return;
        }
        
        if (pivotPrefab == null || linePrefab == null || textPrefab == null)
        {
            Debug.LogError("GrabMoveFeedback: Prefabs not assigned!");
            return;
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("GrabMoveFeedback: Player Transform not assigned!");
            return;
        }
        
        Debug.Log("GrabMoveFeedback: Initializing instances...");
        
        pivotInstance = Instantiate(pivotPrefab);
        lineInstance = Instantiate(linePrefab);
        textInstance = Instantiate(textPrefab);
        
        pivotInstance.transform.SetParent(playerTransform, false);
        lineInstance.transform.SetParent(playerTransform, false);
        textInstance.transform.SetParent(playerTransform, false);
        
        lineRenderer = lineInstance.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GrabMoveFeedback: LineRenderer not found on Line prefab!");
        }
        
        textComponent = textInstance.GetComponentInChildren<TMPro.TextMeshPro>();

        if (textComponent == null)
        {
            Debug.LogError("GrabMoveFeedback: TextMeshProUGUI not found on Text prefab!");
        }

        pivotInstance.SetActive(false);
        lineInstance.SetActive(false);
        textInstance.SetActive(false);
        
        isInitialized = true;
        
        Debug.Log("GrabMoveFeedback: Initialized and hidden!");
    }

    public void UpdatePivotVisual(Vector3 pivotPosition)
    {
        if (pivotInstance != null && playerTransform != null)
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
        if (textInstance != null && textComponent != null && pivotInstance != null)
        {
            float percent = Mathf.Min(worldScalePercent, 100f);

            textComponent.text = worldScalePercent.ToString("F0") + "%";
            
            textInstance.transform.position = pivotInstance.transform.position + Vector3.up * textOffsetY;
            
            if (playerTransform != null)
            {
                float playerScale = playerTransform.localScale.x;
                if (playerScale > 0.001f)
                {
                    float compensatedScale = fixedTextScale / playerScale;
                    textInstance.transform.localScale = Vector3.one * compensatedScale;
                }
            }
            
            
            if (Camera.main != null)
            {
                textInstance.transform.LookAt(Camera.main.transform);
                textInstance.transform.Rotate(0, 180, 0);
            }
        }
    }

    public void SetFeedbackActive(bool isActive)
    {
        if (!isInitialized)
        {
            if (isActive)
            {
                Debug.LogWarning("Trying to activate feedback before initialization!");
            }
            return;
        }
        
        Debug.Log($"GrabMoveFeedback: SetActive({isActive})");
        
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