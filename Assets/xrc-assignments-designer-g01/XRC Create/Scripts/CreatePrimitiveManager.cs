using UnityEngine;

namespace XRC.Toolkit.Create
{
    [RequireComponent(typeof(CreateLogic))]
    public class CreatePrimitiveManager : MonoBehaviour
    {
        private CreateLogic createLogic;

        private void Awake()
        {
            createLogic = GetComponent<CreateLogic>();
            
            if (createLogic == null)
            {
                Debug.LogError("[CreatePrimitiveManager] Could not find CreateLogic!");
            }
            else
            {
                Debug.Log("[CreatePrimitiveManager] Initialized successfully!");
            }
        }

        
        
        public void SetPrimitiveCube()
        {
            SetType(PrimitiveType.Cube);
        }

        public void SetPrimitiveSphere()
        {
            SetType(PrimitiveType.Sphere);
        }

        public void SetPrimitiveCapsule()
        {
            SetType(PrimitiveType.Capsule);
        }

        public void SetPrimitiveCylinder()
        {
            SetType(PrimitiveType.Cylinder);
        }

        public void SetPrimitivePlane()
        {
            SetType(PrimitiveType.Plane);
        }

        public void SetPrimitiveQuad()
        {
            SetType(PrimitiveType.Quad);
        }

        private void SetType(PrimitiveType type)
        {
            if (createLogic != null)
            {
                createLogic.primitiveType = type;
                Debug.Log($"[CreatePrimitiveManager] Type set to: {type}");
            }
            else
            {
                Debug.LogError("[CreatePrimitiveManager] CreateLogic is null!");
            }
        }
    }
}