using UnityEngine;

public class ColorWidget : MonoBehaviour
{
    [Header("Cursor Configuration")]
    [SerializeField] private Transform cursorTransform;
    [SerializeField] private GameObject cursorVisualizationPrefab;
    [SerializeField] private Vector3 cursorOffset = new Vector3(0f, 0f, 0.15f);

    [Header("HSV Cylinder")]
    [SerializeField] private Transform cylinderParent;
    [SerializeField] private GameObject cylinderPrefab;

    [Header("Cylinder Offsets")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0.1f, 0, -0.05f);
    [SerializeField] private Vector3 rotationOffset = new Vector3(45f, 0, 0);

    [Header("Cylinder Dimensions")]
    [SerializeField] private float cylinderRadius = 1f;
    [SerializeField] private float cylinderMaxHeight = 2f;

    private GameObject cursorInstance;
    private GameObject cylinderInstance;
    private Renderer cursorRenderer;
    private Renderer cylinderRenderer;
    private Transform cylinderTransform;

    private float currentHue = 0f;
    private float currentSaturation = 0f;
    private float currentValue = 1f;
    private Color currentColor = Color.white;

    private bool isCursorInside = false;
    private Vector3 cylinderBottomWorld;

    private void Awake()
    {
        if (cursorVisualizationPrefab != null)
        {
            cursorInstance = Instantiate(cursorVisualizationPrefab);
            cursorInstance.SetActive(false);
            cursorRenderer = cursorInstance.GetComponentInChildren<Renderer>();
        }

        if (cylinderPrefab != null)
        {
            cylinderInstance = Instantiate(cylinderPrefab);
            cylinderInstance.SetActive(false);
            cylinderTransform = cylinderInstance.transform;
            cylinderRenderer = cylinderInstance.GetComponentInChildren<Renderer>();

            cylinderTransform.localScale = new Vector3(
                cylinderRadius * 2f, 
                cylinderMaxHeight / 2f, 
                cylinderRadius * 2f
            );
        }
    }

    private void Update()
    {
        if (cylinderInstance == null || !cylinderInstance.activeSelf) return;

        UpdateCylinderBase();
        UpdateCursorPos();
        SampleColorAtCursor();
        UpdateCylinderMaterial();
    }

    public void Show()
    {
        cursorInstance?.SetActive(true);
        cylinderInstance?.SetActive(true);
    }

    public void Hide()
    {
        cursorInstance?.SetActive(false);
        cylinderInstance?.SetActive(false);
    }

    public Color GetCurrentColor() => currentColor;
    public bool IsCursorInsideCylinder() => isCursorInside;

    private void UpdateCylinderBase()
    {
        if (cylinderTransform == null) return;

        if (cylinderParent != null)
        {
            cylinderTransform.rotation = cylinderParent.rotation * Quaternion.Euler(rotationOffset);
            cylinderBottomWorld = cylinderParent.position + cylinderParent.TransformDirection(positionOffset);
            cylinderTransform.position = cylinderBottomWorld + cylinderTransform.up * (cylinderMaxHeight / 2f);
        }
        else
        {
            var cam = Camera.main;
            if (cam)
            {
                cylinderBottomWorld = cam.transform.position + cam.transform.forward * 1f;
                cylinderTransform.rotation = Quaternion.identity;
                cylinderTransform.position = cylinderBottomWorld + cylinderTransform.up * (cylinderMaxHeight / 2f);
            }
        }
    }

    private void UpdateCursorPos()
    {
        if (cursorInstance == null || cursorTransform == null) return;

        cursorInstance.transform.position =
            cursorTransform.position + cursorTransform.TransformDirection(cursorOffset);
    }

    private void SampleColorAtCursor()
    {
        if (!cursorInstance || !cylinderTransform) return;

        Vector3 localPos = cylinderTransform.InverseTransformPoint(cursorInstance.transform.position);
        float radiusXZ = Mathf.Sqrt(localPos.x * localPos.x + localPos.z * localPos.z);
        isCursorInside = true;

        float clampedY = localPos.y;
        if (clampedY < -1.0f)
        {
            clampedY = -0.9f;
        }
        else if (clampedY > 1.0f)
        {
            clampedY = 1.0f;
        }

        currentHue = (Mathf.Atan2(localPos.z, localPos.x) * Mathf.Rad2Deg + 360f) % 360f;
        currentHue /= 360f;
        currentSaturation = Mathf.Clamp01(radiusXZ / cylinderRadius);
        currentValue = Mathf.Clamp01((clampedY + 1.0f) / 2.0f);

        Color pureColorWheel = Color.HSVToRGB(currentHue, currentSaturation, 1.0f);
        currentColor = pureColorWheel * currentValue;

        if (cursorRenderer != null)
        {
            cursorRenderer.material.color = currentColor;
        }
    }

    private void UpdateCylinderMaterial()
    {
        if (cylinderRenderer == null || cursorInstance == null || cylinderTransform == null) return;

        Vector3 cursorLocalPos = cylinderTransform.InverseTransformPoint(cursorInstance.transform.position);
        cylinderRenderer.material.SetVector("_CursorLocalPos", cursorLocalPos);
        cylinderRenderer.material.SetFloat("_Radius", cylinderRadius);
    }

    private void OnDestroy()
    {
        if (cursorInstance) Destroy(cursorInstance);
        if (cylinderInstance) Destroy(cylinderInstance);
    }
}