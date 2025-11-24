using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace XRC.Projects.Designer.SceneLoader
{
    /// <summary>
    /// Handles input for toggling the Scene Loader UI panel.
    /// Follows XRC Toolkit Input component pattern with InputActionProperty.
    /// </summary>
    [AddComponentMenu("XRC/Designer/Scene Loader Input")]
    public class CSceneLoaderInput : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField]
        [Tooltip("Input action to toggle the scene loader menu visibility")]
        InputActionProperty m_ToggleMenuAction;

        [Header("UI References")]
        [SerializeField]
        [Tooltip("UI panel GameObject to show/hide when input is triggered")]
        GameObject m_UIPanel;

        void Start()
        {
            ValidateAndFindUIPanel();

            // Initialize UI state (hidden by default)
            if (m_UIPanel != null)
            {
                m_UIPanel.SetActive(false);
                Debug.Log($"CSceneLoaderInput : Initialized on '{gameObject.name}' with UI Panel '{m_UIPanel.name}'");
            }
        }

        void OnEnable()
        {
            // Re-validate UI Panel when component is enabled (handles returning to Home scene)
            ValidateAndFindUIPanel();

            if (m_ToggleMenuAction.action != null)
            {
                m_ToggleMenuAction.action.Enable();
                m_ToggleMenuAction.action.performed += OnToggleMenu;
            }
        }

        /// <summary>
        /// Validates UI Panel reference and finds it as a child if necessary.
        /// This ensures the persistent instance always references its own Canvas.
        /// </summary>
        void ValidateAndFindUIPanel()
        {
            // If reference is null or points to destroyed object, find Canvas in children
            if (m_UIPanel == null || m_UIPanel.transform.parent != transform)
            {
                Canvas canvas = GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                {
                    m_UIPanel = canvas.gameObject;
                    Debug.Log($"CSceneLoaderInput : Found Canvas child '{m_UIPanel.name}' and updated reference.");
                }
                else
                {
                    Debug.LogError("SceneLoaderInput: UI Panel is not assigned and no Canvas found in children!", this);
                }
            }
        }

        void OnDisable()
        {
            if (m_ToggleMenuAction.action != null)
            {
                m_ToggleMenuAction.action.performed -= OnToggleMenu;
                m_ToggleMenuAction.action.Disable();
            }
        }

        /// <summary>
        /// Called when the toggle menu input action is performed.
        /// </summary>
        /// <param name="context">Input action callback context</param>
        void OnToggleMenu(InputAction.CallbackContext context)
        {
            if (m_UIPanel == null)
            {
                Debug.LogError("CSceneLoaderInput : UI Panel reference is null. Cannot toggle UI.", this);
                return;
            }

            bool newState = !m_UIPanel.activeSelf;
            m_UIPanel.SetActive(newState);
            Debug.Log($"SceneLoaderInput: UI toggled to {(newState ? "visible" : "hidden")}");
        }

        /// <summary>
        /// Programmatically show the UI panel.
        /// </summary>
        public void ShowUI()
        {
            if (m_UIPanel != null)
            {
                m_UIPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Programmatically hide the UI panel.
        /// </summary>
        public void HideUI()
        {
            if (m_UIPanel != null)
            {
                m_UIPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Refresh the input action binding (re-enable after scene changes).
        /// Called by SceneLoaderLogic when duplicate instances are destroyed.
        /// </summary>
        public void RefreshInputAction()
        {
            if (m_ToggleMenuAction.action != null)
            {
                m_ToggleMenuAction.action.Enable();
                Debug.Log("CSceneLoaderInput : Input action refreshed and re-enabled.");
            }
        }
    }
}
