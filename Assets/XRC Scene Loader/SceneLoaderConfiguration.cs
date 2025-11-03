using UnityEngine;

namespace XRC.Projects.Designer.SceneLoader
{
    /// <summary>
    /// Configuration asset for Scene Loader.
    /// Stores list of scenes available for loading.
    /// Create via: Assets > Create > XRC > Designer > Scene Loader Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "SceneLoaderConfiguration", menuName = "XRC/Designer/Scene Loader Configuration")]
    public class SceneLoaderConfiguration : ScriptableObject
    {
        /// <summary>
        /// Represents a single scene entry in the configuration.
        /// </summary>
        [System.Serializable]
        public class SceneEntry
        {
            [Tooltip("Display name shown on UI button")]
            public string displayName;

            [Tooltip("Scene name (must match name in Build Settings)")]
            public string sceneName;
        }

        [Header("Scene List")]
        [Tooltip("List of scenes available for loading")]
        public SceneEntry[] scenes = new SceneEntry[0];

        /// <summary>
        /// Gets the total number of scenes in the configuration.
        /// </summary>
        public int sceneCount => scenes?.Length ?? 0;

        /// <summary>
        /// Gets the scene entry at the specified index.
        /// Returns null if index is out of bounds.
        /// </summary>
        /// <param name="index">Index of the scene to retrieve</param>
        /// <returns>SceneEntry at the specified index, or null if out of bounds</returns>
        public SceneEntry GetSceneAtIndex(int index)
        {
            if (scenes == null || index < 0 || index >= scenes.Length)
                return null;
            return scenes[index];
        }
    }
}
