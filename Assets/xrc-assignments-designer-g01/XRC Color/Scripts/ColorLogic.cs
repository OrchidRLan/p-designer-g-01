namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ColorLogic:MonoBehaviour
    {
        [SerializeField] private GameObject editObject;
        [SerializeField] private Color Color;
        
        private ColorWidget colorWidget;
        private ColorFeedback colorFeedback;

        private Renderer targetrenderer;
        private MaterialPropertyBlock propertyBlock;

        public Color color()
        {
            get => color();
            set => color = value;
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
