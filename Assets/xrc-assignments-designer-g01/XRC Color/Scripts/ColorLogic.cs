namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ColorLogic:MonoBehaviour
    {
        [SerializeField] private GameObject editObject;
        [SerializeField] private Color CurrentColor = Color.white;
        
        private ColorWidget colorWidget;
        private ColorFeedback colorFeedback;

        private Renderer targetrenderer;
        private MaterialPropertyBlock propertyBlock;

        public Color Color
        {
            get => Color;
            set => Color = value;
        }
        
        public void EnterEditMode()
        {
            
        }

        public void ExitEditMode()
        {
            
        }

        public void ColorApply()
        {
            
        }

    }
}
