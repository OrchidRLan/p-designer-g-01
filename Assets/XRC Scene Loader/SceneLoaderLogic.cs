using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XRC.Projects.Designer.SceneLoader
{
    /// <summary>
    /// Core logic for scene loading orchestration.
    /// Manages scene configuration and loading operations.
    /// Does NOT implement IRunnable - scene loading is action-based, not stateful.
    /// </summary>
    [AddComponentMenu("XRC/Designer/Scene Loader Logic")]
    public class SceneLoaderLogic : MonoBehaviour
    {
        static SceneLoaderLogic s_Instance;

        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Scene configuration (list of loadable scenes)")]
        SceneLoaderConfiguration m_Configuration;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Should UI panel be visible on start?")]
        bool m_ShowUIOnStart = false;

        /// <summary>
        /// Invoked when UI visibility should change.
        /// Parameter: isVisible (true = show, false = hide)
        /// </summary>
        public static event Action<bool> uiVisibilityChanged;

        /// <summary>
        /// Invoked when a scene is about to be loaded.
        /// Parameter: sceneName
        /// </summary>
        public static event Action<string> sceneLoadStarted;

        bool m_IsUIVisible;

        /// <summary>
        /// Access the persistent SceneLoaderLogic instance from any scene.
        /// </summary>
        public static SceneLoaderLogic Instance => s_Instance;

        /// <summary>
        /// Gets the current scene configuration.
        /// </summary>
        public SceneLoaderConfiguration configuration => m_Configuration;

        /// <summary>
        /// Gets whether the UI is currently visible.
        /// </summary>
        public bool isUIVisible => m_IsUIVisible;

        void Awake()
        {
            // Implement singleton pattern to prevent duplicates
            if (s_Instance != null && s_Instance != this)
            {
                Debug.Log($"SceneLoaderLogic: Duplicate instance detected in scene '{SceneManager.GetActiveScene().name}'. Destroying duplicate.");
                // Use DestroyImmediate to avoid timing issues when returning to Home scene
                DestroyImmediate(gameObject);

                // Refresh persistent instance's input action after duplicate destruction
                var persistentInput = s_Instance.GetComponent<SceneLoaderInput>();
                if (persistentInput != null)
                {
                    persistentInput.RefreshInputAction();
                }
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"SceneLoaderLogic: Persistent instance created from scene '{SceneManager.GetActiveScene().name}'");
        }

        void Start()
        {
            if (!HasValidConfiguration())
                return;

            // Initialize UI visibility
            if (m_ShowUIOnStart)
                ShowUI();
        }

        void OnDestroy()
        {
            // Clean up static reference if this is the singleton instance
            if (s_Instance == this)
            {
                s_Instance = null;

                // Clear all event subscribers to prevent memory leaks
                uiVisibilityChanged = null;
                sceneLoadStarted = null;
            }
        }

        /// <summary>
        /// Validates that the configuration is not null.
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        bool HasValidConfiguration()
        {
            if (m_Configuration == null)
            {
                Debug.LogError("SceneLoaderLogic requires SceneLoaderConfiguration", this);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Toggle UI panel visibility.
        /// </summary>
        public void ToggleUI()
        {
            if (m_IsUIVisible)
                HideUI();
            else
                ShowUI();
        }

        /// <summary>
        /// Show the UI panel.
        /// </summary>
        public void ShowUI()
        {
            m_IsUIVisible = true;
            uiVisibilityChanged?.Invoke(true);
        }

        /// <summary>
        /// Hide the UI panel.
        /// </summary>
        public void HideUI()
        {
            m_IsUIVisible = false;
            uiVisibilityChanged?.Invoke(false);
        }

        /// <summary>
        /// Load a scene by index from the configuration.
        /// </summary>
        /// <param name="sceneIndex">Index in the configuration's scene list</param>
        public void LoadScene(int sceneIndex)
        {
            if (!HasValidConfiguration())
                return;

            var sceneEntry = m_Configuration.GetSceneAtIndex(sceneIndex);
            if (sceneEntry == null)
            {
                Debug.LogError($"Scene index {sceneIndex} out of range (0-{m_Configuration.sceneCount - 1})", this);
                return;
            }

            LoadSceneByName(sceneEntry.sceneName);
        }

        /// <summary>
        /// Load a scene by name.
        /// </summary>
        /// <param name="sceneName">Name of scene to load (must be in Build Settings)</param>
        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Cannot load scene: Scene name is null or empty", this);
                return;
            }

            sceneLoadStarted?.Invoke(sceneName);

            // Async scene loading to prevent frame stutter
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// Coroutine for async scene loading without blocking the main thread.
        /// </summary>
        System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
