using UnityEngine;

namespace PlayFrame.Systems.Layout
{
    /// <summary>
    /// ScriptableObject configuration for game scene layout.
    /// Defines the zones (top HUD, center game area, bottom controls, side bars)
    /// and their proportional sizes for a consistent game scene structure.
    /// </summary>
    [CreateAssetMenu(fileName = "GameLayoutConfig", menuName = "PlayFrame/Layout/Game Layout Config")]
    public class GameLayoutConfig : ScriptableObject
    {
        [Header("Layout Mode")]
        [Tooltip("Whether to use proportional (percentage) or fixed (pixel) sizing")]
        public LayoutSizeMode sizeMode = LayoutSizeMode.Proportional;

        [Header("Top Panel (HUD: score, moves, target)")]
        [Tooltip("Proportional height (0-1) or fixed pixel height")]
        public float topPanelSize = 0.12f;
        public bool showTopPanel = true;
        public LayoutPadding topPadding;

        [Header("Center Area (Grid / Game Area)")]
        [Tooltip("Center takes remaining space. Use padding to add margins.")]
        public LayoutPadding centerPadding;
        public float centerMinHeight = 0.5f;

        [Header("Bottom Panel (Controls, Boosters)")]
        [Tooltip("Proportional height (0-1) or fixed pixel height")]
        public float bottomPanelSize = 0.1f;
        public bool showBottomPanel = true;
        public LayoutPadding bottomPadding;

        [Header("Left Side Panel")]
        [Tooltip("Proportional width (0-1) or fixed pixel width")]
        public float leftPanelSize = 0f;
        public bool showLeftPanel = false;
        public LayoutPadding leftPadding;

        [Header("Right Side Panel")]
        [Tooltip("Proportional width (0-1) or fixed pixel width")]
        public float rightPanelSize = 0f;
        public bool showRightPanel = false;
        public LayoutPadding rightPadding;

        [Header("Background")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        public Sprite backgroundSprite;

        [Header("Safe Area")]
        [Tooltip("Automatically adjust layout for safe area (notch/cutout)")]
        public bool respectSafeArea = true;

        /// <summary>
        /// Get the proportional rect for the top panel.
        /// Returns anchor-based rect (xMin, yMin, xMax, yMax in 0-1 range).
        /// </summary>
        public LayoutRect GetTopRect()
        {
            if (!showTopPanel) return LayoutRect.Zero;

            float leftOffset = showLeftPanel ? leftPanelSize : 0f;
            float rightOffset = showRightPanel ? rightPanelSize : 0f;

            return new LayoutRect(leftOffset, 1f - topPanelSize, 1f - rightOffset, 1f);
        }

        /// <summary>
        /// Get the proportional rect for the center (game) area.
        /// </summary>
        public LayoutRect GetCenterRect()
        {
            float leftOffset = showLeftPanel ? leftPanelSize : 0f;
            float rightOffset = showRightPanel ? rightPanelSize : 0f;
            float topOffset = showTopPanel ? topPanelSize : 0f;
            float bottomOffset = showBottomPanel ? bottomPanelSize : 0f;

            return new LayoutRect(leftOffset, bottomOffset, 1f - rightOffset, 1f - topOffset);
        }

        /// <summary>
        /// Get the proportional rect for the bottom panel.
        /// </summary>
        public LayoutRect GetBottomRect()
        {
            if (!showBottomPanel) return LayoutRect.Zero;

            float leftOffset = showLeftPanel ? leftPanelSize : 0f;
            float rightOffset = showRightPanel ? rightPanelSize : 0f;

            return new LayoutRect(leftOffset, 0f, 1f - rightOffset, bottomPanelSize);
        }

        /// <summary>
        /// Get the proportional rect for the left panel.
        /// </summary>
        public LayoutRect GetLeftRect()
        {
            if (!showLeftPanel) return LayoutRect.Zero;

            float topOffset = showTopPanel ? topPanelSize : 0f;
            float bottomOffset = showBottomPanel ? bottomPanelSize : 0f;

            return new LayoutRect(0f, bottomOffset, leftPanelSize, 1f - topOffset);
        }

        /// <summary>
        /// Get the proportional rect for the right panel.
        /// </summary>
        public LayoutRect GetRightRect()
        {
            if (!showRightPanel) return LayoutRect.Zero;

            float topOffset = showTopPanel ? topPanelSize : 0f;
            float bottomOffset = showBottomPanel ? bottomPanelSize : 0f;

            return new LayoutRect(1f - rightPanelSize, bottomOffset, 1f, 1f - topOffset);
        }
    }

    public enum LayoutSizeMode
    {
        Proportional,
        Fixed
    }

    [System.Serializable]
    public struct LayoutPadding
    {
        public float left;
        public float right;
        public float top;
        public float bottom;

        public LayoutPadding(float all)
        {
            left = right = top = bottom = all;
        }

        public LayoutPadding(float horizontal, float vertical)
        {
            left = right = horizontal;
            top = bottom = vertical;
        }
    }

    [System.Serializable]
    public struct LayoutRect
    {
        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

        public static readonly LayoutRect Zero = new LayoutRect(0, 0, 0, 0);
        public static readonly LayoutRect Full = new LayoutRect(0, 0, 1, 1);

        public LayoutRect(float xMin, float yMin, float xMax, float yMax)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
        }

        public bool IsZero => xMin == 0 && yMin == 0 && xMax == 0 && yMax == 0;
    }
}
