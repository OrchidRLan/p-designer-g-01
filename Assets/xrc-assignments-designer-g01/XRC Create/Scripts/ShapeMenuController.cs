using UnityEngine;

namespace XRC.Toolkit.Create
{
    public class ShapeMenuController : MonoBehaviour
    {
        [Header("refs")]
        [SerializeField]
        private CreateLogic m_CreateLogic;

        [SerializeField]
        private CreateInput m_CreateInput;

        private void SetShape(PrimitiveType type)
        {
            if (m_CreateLogic == null)
            {
                return;
            }

            m_CreateLogic.primitiveType = type;
            m_CreateLogic.ResetPreview();

            if (m_CreateInput != null)
            {
                m_CreateInput.CloseMenu();
            }
        }

        public void SetCube()     => SetShape(PrimitiveType.Cube);
        public void SetSphere()   => SetShape(PrimitiveType.Sphere);
        public void SetCapsule()  => SetShape(PrimitiveType.Capsule);
        public void SetCylinder() => SetShape(PrimitiveType.Cylinder);
        public void SetPlane()    => SetShape(PrimitiveType.Plane);
        public void SetQuad()     => SetShape(PrimitiveType.Quad);
    }
}