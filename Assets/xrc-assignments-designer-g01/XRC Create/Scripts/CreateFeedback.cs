using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace XRC.Toolkit.Create
{
    [RequireComponent(typeof(CreateLogic))]
    public class CreateFeedback : MonoBehaviour
    {
        [Header("Preview Visuals")]
        [SerializeField]
        [Tooltip("Transparency of preview object (0 = invisible, 1 = opaque)")]
        [Range(0f, 1f)]
        private float m_PreviewAlpha = 0.4f;

        [SerializeField]
        [Tooltip("Transparency of final object (should be 1 for fully opaque)")]
        [Range(0f, 1f)]
        private float m_FinalAlpha = 1f;

        [Header("Haptics")]
        [SerializeField]
        private bool m_SendPreviewHaptics = true;

        [SerializeField]
        private float m_HapticIntensity = 0.4f;

        [SerializeField]
        private float m_HapticDuration = 0.05f;

        [SerializeField]
        private HapticsUtility.Controller m_HapticController = HapticsUtility.Controller.Both;

        private CreateLogic m_CreateLogic;

        private void Awake()
        {
            m_CreateLogic = GetComponent<CreateLogic>();
        }
        
        public void OnPreviewEntered(GameObject previewObject)
        {
            ApplyPreviewMaterial(previewObject, m_PreviewAlpha);
            SendPreviewHaptics();
        }
        
        public void OnPreviewExited(GameObject finalObject)
        {
            RestoreFinalMaterial(finalObject);
        }
        
        public void ApplyPreviewMaterial(GameObject previewObject, float alpha)
        {
            if (previewObject == null)
            {
                return;
            }

            var renderer = previewObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }
            
            Material mat = renderer.material;
            
            EnableTransparency(mat);
            
            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
            
            Debug.Log($"[CreateFeedback] Preview material applied with alpha: {alpha}");
        }
        
        public void RestoreFinalMaterial(GameObject finalObject)
        {
            if (finalObject == null)
            {
                return;
            }

            var renderer = finalObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (m_CreateLogic != null && m_CreateLogic.objectMaterial != null)
            {
                renderer.material = m_CreateLogic.objectMaterial;
                Debug.Log("[CreateFeedback] Applied objectMaterial from CreateLogic");
            }
            else
            {

                Material mat = renderer.material;
                EnableTransparency(mat);
                
                Color color = mat.color;
                color.a = m_FinalAlpha;
                mat.color = color;
                
                Debug.Log("[CreateFeedback] Restored material with full opacity");
            }
        }

        private void EnableTransparency(Material mat)
        {
            if (mat == null)
            {
                return;
            }


            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            else if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }


        private void SendPreviewHaptics()
        {
            if (!m_SendPreviewHaptics)
            {
                return;
            }

            HapticsUtility.SendHapticImpulse(
                m_HapticIntensity,
                m_HapticDuration,
                m_HapticController);
        }

        public void SetPreviewAlpha(float alpha)
        {
            m_PreviewAlpha = Mathf.Clamp01(alpha);
        }
        
        public void SetFinalAlpha(float alpha)
        {
            m_FinalAlpha = Mathf.Clamp01(alpha);
        }
    }
}