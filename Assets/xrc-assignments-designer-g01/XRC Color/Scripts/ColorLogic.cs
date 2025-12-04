namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ColorLogic:MonoBehaviour
    {
        [SerializeField] private GameObject editObject;
        [SerializeField] private Color currentColor = Color.white;
        
        private ColorWidget colorWidget;
        private ColorFeedback colorFeedback;

        private Renderer targetrenderer;
        private MaterialPropertyBlock propertyBlock;

        public Color Color
        {
            get => Color;
            set => Color = value;
        }
        
        public GameObject EditObject
        {
            get => editObject;
            set
            {
                editObject  = value;
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
        
        public void EnterEditMode()
        {
            if (colorWidget != null)
            {
                colorWidget.Show();
            }
                
        }
        public void ExitEditMode()
        {
            if (colorWidget != null)
            {
                colorWidget.Hide();
            }
        }

        public void ApplyColor()
        {
            if (colorWidget != null)
            {
                Color sampledColor = colorWidget.GetCurrentColor();
                currentColor = sampledColor;
            }

            if (colorFeedback != null)
            {
                colorFeedback.PlayApplySound();
            }
        }

        private void UpdateTargetRenderer()
        {
            if (editObject != null)
            {
                targetrenderer = editObject.GetComponent<Renderer>();
            }
            else
            {
                targetrenderer = null;
            }
        }
        
        // public void SetEditObject(GameObject obj)
        // {
        //     EditObject = obj;
        // }
        //

    }
}
