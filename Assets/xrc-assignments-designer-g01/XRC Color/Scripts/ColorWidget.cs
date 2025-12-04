namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    public class ColorWidget
    {

        public void Show()
        {
            
        }
        public void Hide()
        {
            
        }
        public Color GetCurrentColor()
        {
            Color sampled = SampleColorFromCursor();
            return sampled;
        }
    }
}