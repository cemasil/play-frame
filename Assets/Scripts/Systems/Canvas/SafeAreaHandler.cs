using UnityEngine;

namespace PlayFrame.Systems.Canvas
{
    /// <summary>
    /// Handles safe area (notch/cutout) adjustments for modern mobile devices.
    /// Attach to a RectTransform that should respect the safe area.
    /// Typically placed on the root content panel under the Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        [Header("Safe Area Settings")]
        [Tooltip("Apply safe area adjustments on the top edge")]
        [SerializeField] private bool applyTop = true;

        [Tooltip("Apply safe area adjustments on the bottom edge")]
        [SerializeField] private bool applyBottom = true;

        [Tooltip("Apply safe area adjustments on the left edge")]
        [SerializeField] private bool applyLeft = true;

        [Tooltip("Apply safe area adjustments on the right edge")]
        [SerializeField] private bool applyRight = true;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (HasScreenChanged())
            {
                ApplySafeArea();
            }
        }

        private bool HasScreenChanged()
        {
            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            var orientation = Screen.orientation;

            if (safeArea != _lastSafeArea || screenSize != _lastScreenSize || orientation != _lastOrientation)
            {
                _lastSafeArea = safeArea;
                _lastScreenSize = screenSize;
                _lastOrientation = orientation;
                return true;
            }
            return false;
        }

        private void ApplySafeArea()
        {
            var safeArea = Screen.safeArea;

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Only apply to edges that are enabled
            if (!applyLeft) anchorMin.x = 0f;
            if (!applyBottom) anchorMin.y = 0f;
            if (!applyRight) anchorMax.x = 1f;
            if (!applyTop) anchorMax.y = 1f;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;
        }

        /// <summary>
        /// Force refresh the safe area calculation
        /// </summary>
        public void Refresh()
        {
            ApplySafeArea();
        }

        /// <summary>
        /// Configure which edges respect safe area
        /// </summary>
        public void SetEdges(bool top, bool bottom, bool left, bool right)
        {
            applyTop = top;
            applyBottom = bottom;
            applyLeft = left;
            applyRight = right;
            ApplySafeArea();
        }
    }
}
