using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace XRC.Toolkit.Create
{
    [RequireComponent(typeof(CreateLogic))]
    public class CreateFeedback : MonoBehaviour
    {
        [Header("preview visuals")]
        [SerializeField]
        private float m_PreviewAlpha = 0.4f;

        [SerializeField]
        private float m_FinalAlpha = 1f;

        [Header("haptics")]
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

        // called when preview starts
        public void OnPreviewEntered(GameObject previewObject)
        {
            ApplyPreviewMaterial(previewObject, m_PreviewAlpha);
            SendPreviewHaptics();
        }

        // called when preview finalizes
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

            var mat = renderer.material;
            var color = mat.color;
            color.a = alpha;
            mat.color = color;
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

            // use final material from logic if set
            if (m_CreateLogic != null && m_CreateLogic.objectMaterial != null)
            {
                renderer.material = m_CreateLogic.objectMaterial;
            }
            else
            {
                var mat = renderer.material;
                var color = mat.color;
                color.a = m_FinalAlpha;
                mat.color = color;
            }
        }

        private void SendPreviewHaptics()
        {
            if (!m_SendPreviewHaptics)
            {
                return;
            }

            // amplitude, duration, controller
            HapticsUtility.SendHapticImpulse(
                m_HapticIntensity,
                m_HapticDuration,
                m_HapticController);
        }
    }
}
