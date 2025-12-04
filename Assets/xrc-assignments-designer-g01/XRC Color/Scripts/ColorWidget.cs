namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    public class ColorWidget: MonoBehaviour
    {
        [Header("Cursor Configuration")] [SerializeField]
        private Transform cursorTransform;
        private GameObject cursorVisualizationPrefab;
        
        [Header("HSV Cylinder")] [SerializeField]
        private Transform cylinderParent;
        private GameObject cylinderPrefab;
        private Vector3 positionOffset = new Vector3(0.1f, 0f, -0.05f);
        private Vector3 rotationOffset = new Vector3(-45f, 0f, 0f);
        
        private float cylinderRadius = 0.1f;
        private float cylinderMaxHeight = 0.2f;
        
        [Header("Input Actions")]
        [SerializeField] private InputActionReference applyColorAction;

        private GameObject cursorInstance;
        private GameObject cylinderInstance;
        private Renderer cursorRenderer;
        private Renderer cylinderRenderer;
        private Transform cylinderTransform;
        
        private float currentHue = 0f;
        private float currentSaturation = 0f;
        private float currentValue = 0f;
        private Color currentColor = Color.white;
        
        private bool isCursorInside = false;

        private void Awake()
        {
            if (cursorVisualizationPrefab != null)
            {
                cursorInstance = GameObject.Instantiate(cursorVisualizationPrefab);
                cursorInstance.SetActive(false);
                cursorRenderer = cursorInstance.GetComponent<Renderer>();
            }

            if (cylinderPrefab != null)
            {
                cylinderInstance = GameObject.Instantiate(cylinderPrefab);
                cylinderInstance.SetActive(false);
                cylinderRenderer = cylinderInstance.GetComponent<Renderer>();
            }
        }

        private void Update()
        {
            if (!cylinderInstance.activeSelf) return;
            UpdateCylinderPosition();
            UpdateCursorPosition();
            SampleColorAtCursor();
            UpdateCylinderHeight();
        }
        public void Show()
        {
            if (cursorInstance != null)
            {
                cursorInstance.SetActive(true);
            }

            if (cylinderInstance != null)
            {
                cylinderInstance.SetActive(true);
            }
        }
        public void Hide()
        {
            if (cursorInstance != null)
            {
                cursorInstance.SetActive(false);
            }

            if (cylinderInstance != null)
            {
                cylinderInstance.SetActive(false);
            }
            
        }
        public Color GetCurrentColor()
        {
            return currentColor;
        }

        public bool IsCursorInsideCylinder()
        {
            return isCursorInside;
        }

        private void UpdateCylinderPosition()
        {
            if (cylinderTransform == null || cylinderParent == null) return;
            cylinderTransform.position = cylinderParent.position + cylinderParent.TransformDirection(positionOffset);
            cylinderTransform.rotation = cylinderParent.rotation * Quaternion.Euler(rotationOffset);
        }

        private void UpdateCursorPosition()
        {
            if (cursorTransform == null || cursorRenderer == null) return;
            cursorInstance.transform.position = cursorTransform.position + cursorTransform.TransformDirection(positionOffset);
        }

        private void SampleColorAtCursor()
        {
            if (cursorTransform == null || cylinderTransform == null) return;
            Vector3 localCursorPos =
                cylinderTransform.InverseTransformPoint(cursorTransform.position);

            float radiusXZ = Mathf.Sqrt(localCursorPos.x * localCursorPos.x +
                                        localCursorPos.z * localCursorPos.z);
            bool insideRadius = radiusXZ <= cylinderRadius;
            bool insideHeight = localCursorPos.y >= 0f &&
                                localCursorPos.y <= cylinderMaxHeight;

            isCursorInside = insideRadius && insideHeight;

            if (!isCursorInside)
            {
                if (!cursorRenderer != null)
                {
                    cursorRenderer.material.color = Color.grey;
                }

                return;
            }

            currentHue = (Mathf.Atan2(localCursorPos.z, localCursorPos.x) * Mathf.Rad2Deg + 360f) %
                         360f;
            currentHue /= 360f;
            currentSaturation = Mathf.Clamp01(radiusXZ / cylinderRadius);
            currentValue = Mathf.Clamp01(localCursorPos.y / cylinderMaxHeight);
            currentColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);

            if (cursorRenderer != null)
            {
                cursorRenderer.material.color = currentColor;
            }
        }

        private void UpdateCylinderHeight()
        {
            if (cylinderTransform == null) return;
            
            float targetHeight = isCursorInside ? cylinderTransform.localScale.y : 0.01f;

            float scaleY = targetHeight / 2f;
            cylinderTransform.localScale = new Vector3(cylinderRadius * 2f, scaleY, cylinderRadius * 2f);

            Vector3 localPos = cylinderTransform.localPosition;
            localPos.y = targetHeight / 2f;
            cylinderTransform.localPosition = localPos;

            if (cylinderRenderer != null && isCursorInside)
            {
                Color topColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
                cylinderRenderer.material.color = topColor;
            }
        }

        private void OnDestroy()
        {
            if (cursorInstance != null)
            {
                Destroy(cursorInstance);
            }
            
            if (cylinderInstance != null)
            {
                Destroy(cylinderInstance);
            }
        }
    }
}