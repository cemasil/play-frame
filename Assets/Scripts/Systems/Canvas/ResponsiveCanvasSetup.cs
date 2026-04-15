using UnityEngine;
using UnityEngine.UI;

namespace PlayFrame.Systems.Canvas
{
    /// <summary>
    /// Configures CanvasScaler for responsive mobile UI.
    /// Attach to any Canvas GameObject. Automatically sets up scaling
    /// for consistent UI across all mobile device resolutions.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class ResponsiveCanvasSetup : MonoBehaviour
    {
        [Header("Reference Resolution")]
        [Tooltip("Design resolution width (portrait: 1080, landscape: 1920)")]
        [SerializeField] private float referenceWidth = 1080f;

        [Tooltip("Design resolution height (portrait: 1920, landscape: 1080)")]
        [SerializeField] private float referenceHeight = 1920f;

        [Header("Scaling")]
        [Tooltip("0 = match width, 1 = match height, 0.5 = balanced")]
        [Range(0f, 1f)]
        [SerializeField] private float matchWidthOrHeight = 0.5f;

        [Tooltip("Automatically adjust match based on aspect ratio")]
        [SerializeField] private bool autoAdjustMatch = true;

        [Header("Aspect Ratio Thresholds")]
        [Tooltip("Aspect ratios wider than this favor width matching")]
        [SerializeField] private float wideAspectThreshold = 2f;

        [Tooltip("Aspect ratios taller than this favor height matching")]
        [SerializeField] private float tallAspectThreshold = 2.2f;

        private CanvasScaler _canvasScaler;
        private float _lastScreenWidth;
        private float _lastScreenHeight;

        private void Awake()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            ConfigureScaler();
        }

        private void Start()
        {
            UpdateMatchFactor();
        }

        private void Update()
        {
            if (!autoAdjustMatch) return;

            // Only recalculate when screen size changes
            if (Mathf.Approximately(Screen.width, _lastScreenWidth) &&
                Mathf.Approximately(Screen.height, _lastScreenHeight))
                return;

            UpdateMatchFactor();
        }

        private void ConfigureScaler()
        {
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }

        private void UpdateMatchFactor()
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            if (!autoAdjustMatch)
            {
                _canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
                return;
            }

            float screenAspect = (float)Screen.height / Screen.width;
            float referenceAspect = referenceHeight / referenceWidth;

            // For devices wider than reference (e.g., tablets), favor width
            // For devices taller than reference (e.g., modern phones), favor height
            if (screenAspect > tallAspectThreshold)
            {
                // Very tall phone (e.g., 21:9) - match width to prevent UI shrinking
                _canvasScaler.matchWidthOrHeight = 0f;
            }
            else if (screenAspect < 1f / wideAspectThreshold)
            {
                // Very wide device (landscape tablet) - match height
                _canvasScaler.matchWidthOrHeight = 1f;
            }
            else
            {
                // Balanced for most devices
                _canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            }
        }

        /// <summary>
        /// Set reference resolution at runtime
        /// </summary>
        public void SetReferenceResolution(float width, float height)
        {
            referenceWidth = width;
            referenceHeight = height;
            if (_canvasScaler != null)
            {
                _canvasScaler.referenceResolution = new Vector2(width, height);
            }
        }

        /// <summary>
        /// Set match factor at runtime
        /// </summary>
        public void SetMatchFactor(float match)
        {
            matchWidthOrHeight = Mathf.Clamp01(match);
            if (_canvasScaler != null)
            {
                _canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_canvasScaler == null)
                _canvasScaler = GetComponent<CanvasScaler>();

            if (_canvasScaler != null)
            {
                ConfigureScaler();
            }
        }
#endif
    }
}
