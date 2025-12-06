using UnityEngine;
using UnityEngine.SceneManagement;

namespace XRC.Projects.Designer.SceneLoader
{
    public class SceneLoaderUIAttacher : MonoBehaviour
    {
        [SerializeField] Canvas m_UICanvas;
        [SerializeField] string m_ControllerName = "LeftHand Controller";
        [SerializeField] Vector3 m_LocalPositionOffset = Vector3.zero;
        [SerializeField] Vector3 m_LocalRotationOffset = Vector3.zero;
        [SerializeField] Vector3 m_LocalScale = Vector3.one;
        [SerializeField] ScaleCompensation m_ScaleCompensation;
        [SerializeField] bool m_AutoFindScaleCompensation = true;

        Transform m_TrackedController;
        bool m_IsRegisteredWithCompensation = false;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnregisterFromScaleCompensation();
        }

        void Start()
        {
            if (m_ScaleCompensation == null && m_AutoFindScaleCompensation)
            {
                m_ScaleCompensation = FindObjectOfType<ScaleCompensation>();
            }

            if (m_UICanvas != null && m_UICanvas.transform.parent != transform)
            {
                m_UICanvas.transform.SetParent(transform, true);
            }

            AttachToLeftController();
            RegisterWithScaleCompensation();
        }

        void LateUpdate()
        {
            if (m_UICanvas == null) return;

            if (m_TrackedController != null)
            {
                UpdateCanvasTransform();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (m_UICanvas == null) return;

            if (m_UICanvas.transform.parent != transform)
            {
                m_UICanvas.transform.SetParent(transform, true);
            }

            AttachToLeftController();
        }

        void AttachToLeftController()
        {
            if (m_UICanvas == null) return;

            Transform leftController = FindLeftController();

            if (leftController == null)
            {
                m_TrackedController = null;
                return;
            }

            m_TrackedController = leftController;
            UpdateCanvasTransform();
        }

        void UpdateCanvasTransform()
        {
            if (m_TrackedController == null || m_UICanvas == null)
                return;

            Quaternion offsetRotation = Quaternion.Euler(m_LocalRotationOffset);
            m_UICanvas.transform.position = m_TrackedController.position + m_TrackedController.rotation * m_LocalPositionOffset;
            m_UICanvas.transform.rotation = m_TrackedController.rotation * offsetRotation;
            
            Vector3 finalScale = m_LocalScale;
            if (m_ScaleCompensation != null)
            {
                float compensationFactor = m_ScaleCompensation.GetCompensationFactor();
                finalScale = m_LocalScale * compensationFactor;
            }
            m_UICanvas.transform.localScale = finalScale;
        }

        Transform FindLeftController()
        {
            GameObject controllerObject = GameObject.Find(m_ControllerName);
            return controllerObject != null ? controllerObject.transform : null;
        }

        void RegisterWithScaleCompensation()
        {
            if (m_ScaleCompensation != null && m_UICanvas != null && !m_IsRegisteredWithCompensation)
            {
                m_IsRegisteredWithCompensation = true;
            }
        }

        void UnregisterFromScaleCompensation()
        {
            m_IsRegisteredWithCompensation = false;
        }

        public void ReattachUI()
        {
            AttachToLeftController();
        }
    }
}