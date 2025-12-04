namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    public class ColorWidget
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
            Vector3 localPositon =
                cylinderTransform.InverseTransformDirection(cursorTransform.position);
            
            float radiusXZ == Mathf.Sqrt(localCursorPositon.x )

        }

        private void UpdateCylinderHeight()
        {
            
        }
    }
}