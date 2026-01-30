using UnityEngine;

namespace PlayFrame.Systems.Scene
{
    /// <summary>
    /// Configuration settings for scene loading behavior.
    /// Create via: Assets > Create > PlayFrame > Scene Loading Settings
    /// </summary>
    [CreateAssetMenu(fileName = "SceneLoadingSettings", menuName = "PlayFrame/Scene Loading Settings")]
    public class SceneLoadingSettings : ScriptableObject
    {
        [Header("Timing")]
        [Tooltip("Delay before starting the actual scene load (seconds)")]
        [SerializeField, Range(0f, 1f)] private float preLoadDelay = 0.1f;

        [Tooltip("Minimum time to show loading screen after load completes (seconds)")]
        [SerializeField, Range(0f, 3f)] private float postLoadDelay = 0.5f;

        [Tooltip("Minimum total loading time to prevent flickering (seconds)")]
        [SerializeField, Range(0f, 5f)] private float minimumLoadDuration = 0f;

        [Header("Progress")]
        [Tooltip("Unity reports 0.9 as complete, this normalizes to 1.0")]
        [SerializeField, Range(0.1f, 1f)] private float progressCompletionThreshold = 0.9f;

        [Tooltip("Smooth the progress bar movement")]
        [SerializeField] private bool smoothProgress = true;

        [Tooltip("Progress smoothing speed (higher = faster)")]
        [SerializeField, Range(1f, 20f)] private float progressSmoothSpeed = 5f;

        // Public accessors
        public float PreLoadDelay => preLoadDelay;
        public float PostLoadDelay => postLoadDelay;
        public float MinimumLoadDuration => minimumLoadDuration;
        public float ProgressCompletionThreshold => progressCompletionThreshold;
        public bool SmoothProgress => smoothProgress;
        public float ProgressSmoothSpeed => progressSmoothSpeed;

        #region Cached Wait Objects
        private WaitForSecondsRealtime _preLoadWait;
        private WaitForSecondsRealtime _postLoadWait;

        /// <summary>
        /// Get cached WaitForSecondsRealtime for pre-load delay
        /// </summary>
        public WaitForSecondsRealtime PreLoadWait
        {
            get
            {
                if (_preLoadWait == null && preLoadDelay > 0f)
                {
                    _preLoadWait = new WaitForSecondsRealtime(preLoadDelay);
                }
                return _preLoadWait;
            }
        }

        /// <summary>
        /// Get cached WaitForSecondsRealtime for post-load delay
        /// </summary>
        public WaitForSecondsRealtime PostLoadWait
        {
            get
            {
                if (_postLoadWait == null && postLoadDelay > 0f)
                {
                    _postLoadWait = new WaitForSecondsRealtime(postLoadDelay);
                }
                return _postLoadWait;
            }
        }

        private void OnValidate()
        {
            // Invalidate cache when values change in Inspector
            _preLoadWait = null;
            _postLoadWait = null;
        }
        #endregion

        /// <summary>
        /// Normalize raw async progress (0-0.9) to display progress (0-1)
        /// </summary>
        public float NormalizeProgress(float rawProgress)
        {
            return Mathf.Clamp01(rawProgress / progressCompletionThreshold);
        }

        /// <summary>
        /// Check if loading is considered complete
        /// </summary>
        public bool IsLoadComplete(float rawProgress)
        {
            return rawProgress >= progressCompletionThreshold;
        }

        /// <summary>
        /// Create default settings instance when no settings asset is assigned.
        /// For production, assign SceneLoadingSettings via Inspector.
        /// </summary>
        public static SceneLoadingSettings CreateDefault()
        {
            var settings = CreateInstance<SceneLoadingSettings>();
            Debug.LogWarning("[SceneLoadingSettings] No SceneLoadingSettings assigned. Using default settings. " +
                           "Create one via Assets > Create > PlayFrame > Scene Loading Settings.");
            return settings;
        }
    }
}
