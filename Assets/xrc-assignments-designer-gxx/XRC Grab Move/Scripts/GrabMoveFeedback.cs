using UnityEngine;
using TMPro;

public class GrabMoveFeedback : MonoBehaviour
{
    [SerializeField] private GameObject pivotPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform playerTransform;
    
    private GameObject pivotInstance;
    private GameObject lineInstance;
    private GameObject textInstance;
    
    private LineRenderer lineRenderer;
    private TextMeshProUGUI textComponent;
    
    public void InitializeFeedbackInstances()
    {
        pivotInstance = Instantiate(pivotPrefab);
        lineInstance = Instantiate(linePrefab);
        textInstance = Instantiate(textPrefab);

        pivotInstance.transform.parent = playerTransform;
        lineInstance.transform.parent = playerTransform;
        textInstance.transform.parent = playerTransform;
        
        lineRenderer = lineInstance.GetComponent<LineRenderer>();
        textComponent = textInstance.GetComponentInChildren<TextMeshProUGUI>();
        
        SetFeedbackActive(false);
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
        if (textInstance != null && textComponent != null)
        {
            textComponent.text = worldScalePercent.ToString("F0") + "%";
            textInstance.transform.position = pivotInstance.transform.position + Vector3.up * 0.2f;
            textInstance.transform.LookAt(Camera.main.transform);
            textInstance.transform.Rotate(0, 180, 0);
        }
    }

    public void SetFeedbackActive(bool isActive)
    {
        if (pivotInstance != null)
            pivotInstance.SetActive(isActive);
        if (lineInstance != null)
            lineInstance.SetActive(isActive);
        if (textInstance != null)
            textInstance.SetActive(isActive);
    }
}