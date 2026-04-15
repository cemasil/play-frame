using UnityEngine;
using UnityEngine.UI;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Layout
{
    /// <summary>
    /// Manages game scene layout by positioning zone RectTransforms according to GameLayoutConfig.
    /// Attach to the root Canvas or a child RectTransform that fills the screen.
    /// Assign zone containers via Inspector; the manager will position them on Awake/Apply.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GameLayoutManager : MonoBehaviour
    {
        private static readonly ILogger _logger = LoggerFactory.CreateGame("GameLayoutManager");

        [Header("Configuration")]
        [SerializeField] private GameLayoutConfig layoutConfig;

        [Header("Zone Containers")]
        [Tooltip("RectTransform for the top HUD (score, moves, target)")]
        [SerializeField] private RectTransform topPanel;

        [Tooltip("RectTransform for the center game area (grid)")]
        [SerializeField] private RectTransform centerPanel;

        [Tooltip("RectTransform for the bottom controls (boosters, buttons)")]
        [SerializeField] private RectTransform bottomPanel;

        [Tooltip("RectTransform for the left side panel")]
        [SerializeField] private RectTransform leftPanel;

        [Tooltip("RectTransform for the right side panel")]
        [SerializeField] private RectTransform rightPanel;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;

        [Header("Options")]
        [SerializeField] private bool applyOnAwake = true;
        [SerializeField] private bool updateOnScreenChange = true;

        private Vector2Int _lastScreenSize;
        private RectTransform _rootRect;

        public GameLayoutConfig Config => layoutConfig;
        public RectTransform TopPanel => topPanel;
        public RectTransform CenterPanel => centerPanel;
        public RectTransform BottomPanel => bottomPanel;
        public RectTransform LeftPanel => leftPanel;
        public RectTransform RightPanel => rightPanel;

        private void Awake()
        {
            _rootRect = GetComponent<RectTransform>();
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (applyOnAwake && layoutConfig != null)
            {
                ApplyLayout();
            }
        }

        private void Update()
        {
            if (!updateOnScreenChange || layoutConfig == null) return;

            var currentSize = new Vector2Int(Screen.width, Screen.height);
            if (currentSize != _lastScreenSize)
            {
                _lastScreenSize = currentSize;
                ApplyLayout();
            }
        }

        /// <summary>
        /// Apply the layout configuration to all zone panels.
        /// </summary>
        public void ApplyLayout()
        {
            if (layoutConfig == null)
            {
                _logger.LogWarning("No layout config assigned");
                return;
            }

            ApplyBackground();
            ApplyZone(topPanel, layoutConfig.GetTopRect(), layoutConfig.topPadding, layoutConfig.showTopPanel);
            ApplyZone(centerPanel, layoutConfig.GetCenterRect(), layoutConfig.centerPadding, true);
            ApplyZone(bottomPanel, layoutConfig.GetBottomRect(), layoutConfig.bottomPadding, layoutConfig.showBottomPanel);
            ApplyZone(leftPanel, layoutConfig.GetLeftRect(), layoutConfig.leftPadding, layoutConfig.showLeftPanel);
            ApplyZone(rightPanel, layoutConfig.GetRightRect(), layoutConfig.rightPadding, layoutConfig.showRightPanel);

            if (layoutConfig.respectSafeArea)
                ApplySafeAreaOffset();

            _logger.Log("Layout applied");
        }

        /// <summary>
        /// Apply a different layout config at runtime.
        /// </summary>
        public void SetConfig(GameLayoutConfig config)
        {
            layoutConfig = config;
            ApplyLayout();
        }

        private void ApplyZone(RectTransform zone, LayoutRect rect, LayoutPadding padding, bool visible)
        {
            if (zone == null) return;

            zone.gameObject.SetActive(visible);

            if (!visible || rect.IsZero) return;

            if (layoutConfig.sizeMode == LayoutSizeMode.Proportional)
            {
                // Use anchors for proportional layout
                zone.anchorMin = new Vector2(rect.xMin, rect.yMin);
                zone.anchorMax = new Vector2(rect.xMax, rect.yMax);
                zone.offsetMin = new Vector2(padding.left, padding.bottom);
                zone.offsetMax = new Vector2(-padding.right, -padding.top);
            }
            else
            {
                // Fixed pixel sizing - use anchor stretch + offset
                zone.anchorMin = new Vector2(rect.xMin, rect.yMin);
                zone.anchorMax = new Vector2(rect.xMax, rect.yMax);

                Rect parentRect = _rootRect.rect;
                float pxLeft = rect.xMin * parentRect.width + padding.left;
                float pxBottom = rect.yMin * parentRect.height + padding.bottom;
                float pxRight = (1f - rect.xMax) * parentRect.width + padding.right;
                float pxTop = (1f - rect.yMax) * parentRect.height + padding.top;

                zone.offsetMin = new Vector2(padding.left, padding.bottom);
                zone.offsetMax = new Vector2(-padding.right, -padding.top);
            }
        }

        private void ApplyBackground()
        {
            if (backgroundImage == null) return;

            backgroundImage.color = layoutConfig.backgroundColor;

            if (layoutConfig.backgroundSprite != null)
            {
                backgroundImage.sprite = layoutConfig.backgroundSprite;
                backgroundImage.type = Image.Type.Sliced;
            }
        }

        private void ApplySafeAreaOffset()
        {
            Rect safeArea = Screen.safeArea;
            float screenW = Screen.width;
            float screenH = Screen.height;

            // If safe area equals full screen, no adjustment needed
            if (Mathf.Approximately(safeArea.x, 0f) && Mathf.Approximately(safeArea.y, 0f) &&
                Mathf.Approximately(safeArea.width, screenW) && Mathf.Approximately(safeArea.height, screenH))
                return;

            // Calculate safe area as anchor offsets
            float safeMinX = safeArea.x / screenW;
            float safeMinY = safeArea.y / screenH;
            float safeMaxX = (safeArea.x + safeArea.width) / screenW;
            float safeMaxY = (safeArea.y + safeArea.height) / screenH;

            // Only adjust the top panel for notch
            if (topPanel != null && layoutConfig.showTopPanel)
            {
                var current = topPanel.anchorMax;
                topPanel.anchorMax = new Vector2(current.x, Mathf.Min(current.y, safeMaxY));
            }

            // Adjust bottom panel for home indicator
            if (bottomPanel != null && layoutConfig.showBottomPanel)
            {
                var current = bottomPanel.anchorMin;
                bottomPanel.anchorMin = new Vector2(current.x, Mathf.Max(current.y, safeMinY));
            }

            // Adjust side panels for notch in landscape
            if (leftPanel != null && layoutConfig.showLeftPanel)
            {
                var current = leftPanel.anchorMin;
                leftPanel.anchorMin = new Vector2(Mathf.Max(current.x, safeMinX), current.y);
            }

            if (rightPanel != null && layoutConfig.showRightPanel)
            {
                var current = rightPanel.anchorMax;
                rightPanel.anchorMax = new Vector2(Mathf.Min(current.x, safeMaxX), current.y);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Layout (Editor)")]
        private void ApplyLayoutEditor()
        {
            _rootRect = GetComponent<RectTransform>();
            ApplyLayout();
        }
#endif
    }
}
