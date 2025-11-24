using UnityEngine;

namespace XRC.Toolkit.Create
{
    public class CreateLogic : MonoBehaviour
    {
        [Header("creation settings")]
        [SerializeField]
        private PrimitiveType m_PrimitiveType = PrimitiveType.Cube;

        [SerializeField]
        private float m_MinScale = 0.05f;

        [SerializeField]
        private float m_MaxScale = 2f;

        [Header("materials")]
        [SerializeField]
        private Material m_ObjectMaterial;

        [SerializeField]
        private Material m_PreviewMaterial;

        //preview state
        private GameObject m_PreviewObject;
        private bool m_IsPreviewActive;
        private bool m_UseUniformScaling = true;

        //simple public api
        public PrimitiveType primitiveType
        {
            get => m_PrimitiveType;
            set => m_PrimitiveType = value;
        }

        public Material objectMaterial
        {
            get => m_ObjectMaterial;
            set => m_ObjectMaterial = value;
        }

        public Material previewMaterial
        {
            get => m_PreviewMaterial;
            set => m_PreviewMaterial = value;
        }

        public bool useUniformScaling
        {
            get => m_UseUniformScaling;
            set => m_UseUniformScaling = value;
        }

        public bool isPreviewActive => m_IsPreviewActive;

        //expose preview object for feedback
        public GameObject previewObject => m_PreviewObject;

        //called while create input is held down
        public void UpdatePreviewTransform(Vector3 leftPos, Vector3 rightPos, Transform origin)
        {
            EnsurePreviewObjectExists();

            if (!m_IsPreviewActive)
            {
                m_IsPreviewActive = true;
                m_PreviewObject.SetActive(true);
            }

            //position at midpoint
            var center = (leftPos + rightPos) * 0.5f;
            m_PreviewObject.transform.position = center;

            //orientation along line between hands, upright to origin
            var direction = rightPos - leftPos;
            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                var up = origin != null ? origin.up : Vector3.up;
                m_PreviewObject.transform.rotation =
                    Quaternion.LookRotation(direction.normalized, up);
            }

            //scale
            if (m_UseUniformScaling)
            {
                ApplyUniformScale(leftPos, rightPos);
            }
            else
            {
                ApplyNonUniformScale(leftPos, rightPos);
            }
        }

        //uniform scale: same value on all axes
        public void ApplyUniformScale(Vector3 leftPos, Vector3 rightPos)
        {
            var delta = rightPos - leftPos;
            var absX = Mathf.Abs(delta.x);
            var absY = Mathf.Abs(delta.y);
            var absZ = Mathf.Abs(delta.z);

            var maxAxis = Mathf.Max(absX, Mathf.Max(absY, absZ));
            var clamped = Mathf.Clamp(maxAxis, m_MinScale, m_MaxScale);

            m_PreviewObject.transform.localScale = new Vector3(clamped, clamped, clamped);
        }

        //non-uniform scale, each axis from controller delta
        public void ApplyNonUniformScale(Vector3 leftPos, Vector3 rightPos)
        {
            var delta = rightPos - leftPos;

            var x = Mathf.Clamp(Mathf.Abs(delta.x), m_MinScale, m_MaxScale);
            var y = Mathf.Clamp(Mathf.Abs(delta.y), m_MinScale, m_MaxScale);
            var z = Mathf.Clamp(Mathf.Abs(delta.z), m_MinScale, m_MaxScale);

            m_PreviewObject.transform.localScale = new Vector3(x, y, z);
        }

        //finalize and spawn a real object from the preview
        public GameObject FinalizeCreateFromPreview()
        {
            if (!m_IsPreviewActive || m_PreviewObject == null)
            {
                return null;
            }

            var finalObject = Instantiate(
                m_PreviewObject,
                m_PreviewObject.transform.position,
                m_PreviewObject.transform.rotation);

            finalObject.transform.localScale = m_PreviewObject.transform.localScale;

            //apply final material if set
            if (m_ObjectMaterial != null)
            {
                var renderer = finalObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = m_ObjectMaterial;
                }
            }

            m_PreviewObject.SetActive(false);
            m_IsPreviewActive = false;

            return finalObject;
        }

        //hide preview without creating anything
        public void CancelPreview()
        {
            if (!m_IsPreviewActive || m_PreviewObject == null)
            {
                return;
            }

            m_PreviewObject.SetActive(false);
            m_IsPreviewActive = false;
        }

        //create the preview object once and reuse it
        private void EnsurePreviewObjectExists()
        {
            if (m_PreviewObject != null)
            {
                return;
            }

            m_PreviewObject = GameObject.CreatePrimitive(m_PrimitiveType);
            m_PreviewObject.name = "xrc create preview";

            //basic preview material 
            if (m_PreviewMaterial != null)
            {
                var renderer = m_PreviewObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = m_PreviewMaterial;
                }
            }

            m_PreviewObject.SetActive(false);
        }
    }
}
