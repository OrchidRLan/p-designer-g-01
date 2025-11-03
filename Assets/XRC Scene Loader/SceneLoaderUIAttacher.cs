using UnityEngine;
using UnityEngine.SceneManagement;

namespace XRC.Projects.Designer.SceneLoader
{
    /// <summary>
    /// Handles UI positioning relative to per-scene XR Origins.
    /// Tracks the left controller and updates UI Canvas transform every frame.
    /// Canvas stays in persistent hierarchy to avoid destruction on scene load.
    /// Note: GameObject persistence is managed by SceneLoaderLogic component.
    /// </summary>
    [AddComponentMenu("XRC/Designer/Scene Loader UI Attacher")]
    public class SceneLoaderUIAttacher : MonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField]
        [Tooltip("Canvas to attach to left controller (typically child of this GameObject)")]
        Canvas m_UICanvas;

        [Header("Attachment Settings")]
        [SerializeField]
        [Tooltip("Name of the GameObject to search for in each scene (default: 'LeftHand Controller')")]
        string m_ControllerName = "LeftHand Controller";

        [SerializeField]
        [Tooltip("Local position offset from controller")]
        Vector3 m_LocalPositionOffset = Vector3.zero;

        [SerializeField]
        [Tooltip("Local rotation offset from controller (Euler angles)")]
        Vector3 m_LocalRotationOffset = Vector3.zero;

        [SerializeField]
        [Tooltip("Local scale")]
        Vector3 m_LocalScale = Vector3.one;

        Transform m_TrackedController;

        void OnEnable()
        {
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            // Unsubscribe from scene loaded event
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            // Ensure Canvas stays in persistent hierarchy (child of this GameObject)
            if (m_UICanvas != null && m_UICanvas.transform.parent != transform)
            {
                Debug.LogWarning($"SceneLoaderUIAttacher: Canvas is not a child of {gameObject.name}. Re-parenting to maintain persistence.");
                m_UICanvas.transform.SetParent(transform, true);
            }

            // Find and track current scene's left controller on startup
            AttachToLeftController();
        }

        void LateUpdate()
        {
            // Validate Canvas reference hasn't been lost
            if (m_UICanvas == null)
            {
                Debug.LogError("SceneLoaderUIAttacher: Canvas reference lost! This should not happen.", this);
                return;
            }

            // Update Canvas transform to follow tracked controller every frame
            if (m_TrackedController != null)
            {
                UpdateCanvasTransform();
            }
        }

        /// <summary>
        /// Called when a new scene is loaded.
        /// </summary>
        /// <param name="scene">The scene that was loaded</param>
        /// <param name="mode">The load mode (single or additive)</param>
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"SceneLoaderUIAttacher: Scene '{scene.name}' loaded. Re-attaching UI to new scene's controller.");

            // Validate Canvas reference after scene load
            if (m_UICanvas == null)
            {
                Debug.LogError("SceneLoaderUIAttacher: Canvas reference is null after scene load! This indicates the Canvas was destroyed.", this);
                return;
            }

            // Ensure Canvas is still in persistent hierarchy after scene load
            if (m_UICanvas.transform.parent != transform)
            {
                Debug.LogWarning($"SceneLoaderUIAttacher: Canvas parent changed to '{m_UICanvas.transform.parent?.name ?? "null"}'. Re-parenting to {gameObject.name} to maintain persistence.");
                m_UICanvas.transform.SetParent(transform, true);
            }

            // Find and track new scene's left controller
            AttachToLeftController();
        }

        /// <summary>
        /// Find the left controller in the current scene and start tracking it.
        /// Canvas remains in persistent hierarchy and follows controller via transform updates.
        /// </summary>
        void AttachToLeftController()
        {
            if (m_UICanvas == null)
            {
                Debug.LogError("SceneLoaderUIAttacher: UI Canvas is not assigned", this);
                return;
            }

            // Find left controller by name in the current scene
            Transform leftController = FindLeftController();

            if (leftController == null)
            {
                Debug.LogWarning($"SceneLoaderUIAttacher: GameObject '{m_ControllerName}' not found in scene '{SceneManager.GetActiveScene().name}'. " +
                    $"UI will not be attached to controller. Please ensure your XR Origin has a child GameObject named '{m_ControllerName}'.");
                m_TrackedController = null;
                return;
            }

            // Store reference to controller for tracking
            m_TrackedController = leftController;

            // Immediately update Canvas transform to match controller
            UpdateCanvasTransform();

            Debug.Log($"SceneLoaderUIAttacher: UI Canvas now tracking '{m_ControllerName}' in scene '{SceneManager.GetActiveScene().name}'");
        }

        /// <summary>
        /// Update Canvas world transform to follow the tracked controller.
        /// Applies configured offsets relative to controller's transform.
        /// </summary>
        void UpdateCanvasTransform()
        {
            if (m_TrackedController == null || m_UICanvas == null)
                return;

            // Calculate world transform based on controller + offsets
            Quaternion offsetRotation = Quaternion.Euler(m_LocalRotationOffset);
            m_UICanvas.transform.position = m_TrackedController.position + m_TrackedController.rotation * m_LocalPositionOffset;
            m_UICanvas.transform.rotation = m_TrackedController.rotation * offsetRotation;
            m_UICanvas.transform.localScale = m_LocalScale;
        }

        /// <summary>
        /// Find the left controller GameObject by name in the current scene.
        /// Uses GameObject.Find() which searches the entire scene hierarchy.
        /// </summary>
        /// <returns>Transform of the left controller, or null if not found</returns>
        Transform FindLeftController()
        {
            GameObject controllerObject = GameObject.Find(m_ControllerName);
            return controllerObject != null ? controllerObject.transform : null;
        }

        /// <summary>
        /// Manually trigger re-attachment (useful for debugging or runtime changes).
        /// </summary>
        public void ReattachUI()
        {
            AttachToLeftController();
        }
    }
}
