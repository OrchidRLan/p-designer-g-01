using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XRC.Projects.Designer.SceneLoader
{
    /// <summary>
    /// Manages Scene Loader UI panel visibility and button generation.
    /// Follows XRC Toolkit Feedback component pattern.
    /// </summary>
    [RequireComponent(typeof(SceneLoaderLogic))]
    [AddComponentMenu("XRC/Designer/Scene Loader UI")]
    public class SceneLoaderUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        [Tooltip("Root UI panel GameObject (will be shown/hidden)")]
        GameObject m_UIPanel;

        [SerializeField]
        [Tooltip("Container for dynamically generated scene buttons")]
        Transform m_ButtonContainer;

        [SerializeField]
        [Tooltip("Prefab for scene buttons (must have Button + TextMeshProUGUI components)")]
        GameObject m_SceneButtonPrefab;

        SceneLoaderLogic m_Logic;

        void Start()
        {
            m_Logic = GetComponent<SceneLoaderLogic>();

            if (m_Logic == null)
            {
                Debug.LogError("SceneLoaderUI requires SceneLoaderLogic component", this);
                return;
            }

            // Subscribe to events
            SceneLoaderLogic.uiVisibilityChanged += OnUIVisibilityChanged;

            // Generate buttons from configuration
            GenerateSceneButtons();

            // Initialize panel state (hide by default)
            if (m_UIPanel != null)
            {
                m_UIPanel.SetActive(m_Logic.isUIVisible);
            }
        }

        void OnDestroy()
        {
            // Clean up event subscriptions
            SceneLoaderLogic.uiVisibilityChanged -= OnUIVisibilityChanged;

            // Clean up all button listeners on component destruction
            if (m_ButtonContainer != null)
            {
                foreach (Transform child in m_ButtonContainer)
                {
                    var button = child.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }
        }

        /// <summary>
        /// Called when UI visibility changes.
        /// </summary>
        /// <param name="isVisible">Whether the UI should be visible</param>
        void OnUIVisibilityChanged(bool isVisible)
        {
            if (m_UIPanel != null)
            {
                m_UIPanel.SetActive(isVisible);
            }
        }

        /// <summary>
        /// Generate scene buttons from the configuration.
        /// </summary>
        void GenerateSceneButtons()
        {
            if (m_Logic == null || m_Logic.configuration == null)
            {
                Debug.LogError("Cannot generate scene buttons: Missing logic or configuration", this);
                return;
            }

            if (m_ButtonContainer == null)
            {
                Debug.LogWarning("Cannot generate scene buttons: Button container is not assigned", this);
                return;
            }

            if (m_SceneButtonPrefab == null)
            {
                Debug.LogWarning("Cannot generate scene buttons: Scene button prefab is not assigned", this);
                return;
            }

            // Clear existing buttons and their listeners
            foreach (Transform child in m_ButtonContainer)
            {
                var button = child.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
                Destroy(child.gameObject);
            }

            // Generate button for each scene in configuration
            var scenes = m_Logic.configuration.scenes;
            if (scenes == null)
            {
                Debug.LogWarning("Configuration has no scenes defined", this);
                return;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                CreateSceneButton(i, scenes[i]);
            }
        }

        /// <summary>
        /// Create a button for a specific scene entry.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene in the configuration</param>
        /// <param name="sceneEntry">Scene entry data</param>
        void CreateSceneButton(int sceneIndex, SceneLoaderConfiguration.SceneEntry sceneEntry)
        {
            if (sceneEntry == null)
            {
                Debug.LogWarning($"Scene entry at index {sceneIndex} is null", this);
                return;
            }

            GameObject buttonObj = Instantiate(m_SceneButtonPrefab, m_ButtonContainer);
            buttonObj.name = $"Button_{sceneEntry.sceneName}";

            // Set button text
            var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = sceneEntry.displayName;
            }
            else
            {
                Debug.LogWarning($"Scene button prefab does not have a TextMeshProUGUI component. Button text will not be set.", this);
            }

            // Wire button click to load scene
            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int capturedIndex = sceneIndex; // Capture for closure
                button.onClick.AddListener(() => OnSceneButtonClicked(capturedIndex));
            }
            else
            {
                Debug.LogWarning($"Scene button prefab does not have a Button component. Button will not be clickable.", this);
            }
        }

        /// <summary>
        /// Called when a scene button is clicked.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to load</param>
        void OnSceneButtonClicked(int sceneIndex)
        {
            if (m_Logic != null)
            {
                m_Logic.LoadScene(sceneIndex);
            }
        }

        /// <summary>
        /// Regenerate scene buttons (call if configuration changes at runtime).
        /// </summary>
        public void RefreshButtons()
        {
            GenerateSceneButtons();
        }
    }
}
