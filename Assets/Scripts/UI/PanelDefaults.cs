using UnityEngine;

namespace PlayFrame.UI
{
    /// <summary>
    /// Global default settings for all panels.
    /// Create via: Assets → Create → PlayFrame → UI → Panel Defaults
    /// Individual panels can override these settings.
    /// </summary>
    [CreateAssetMenu(fileName = "PanelDefaults", menuName = "PlayFrame/UI/Panel Defaults")]
    public class PanelDefaults : ScriptableObject
    {
        [Header("Display")]
        [Tooltip("Default display mode for panels")]
        public PanelDisplayMode displayMode = PanelDisplayMode.Popup;

        [Tooltip("Default popup width ratio (0-1, relative to screen width)")]
        [Range(0.5f, 1f)]
        public float popupWidthRatio = 0.85f;

        [Tooltip("Default popup height ratio (0-1, relative to screen height)")]
        [Range(0.3f, 1f)]
        public float popupHeightRatio = 0.7f;

        [Header("Animation")]
        [Tooltip("Default open animation type")]
        public PanelAnimation openAnimation = PanelAnimation.FadeIn;

        [Tooltip("Default close animation type")]
        public PanelAnimation closeAnimation = PanelAnimation.FadeOut;

        [Tooltip("Default animation duration")]
        [Range(0f, 1f)]
        public float animationDuration = 0.3f;

        [Tooltip("Animation easing curve")]
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Overlay")]
        [Tooltip("Show dark overlay behind popup panels")]
        public bool showOverlay = true;

        [Tooltip("Overlay color")]
        public Color overlayColor = new Color(0f, 0f, 0f, 0.6f);

        [Tooltip("Close panel when overlay is tapped")]
        public bool closeOnOverlayTap = false;

        [Header("Audio")]
        [Tooltip("Sound played when any panel opens")]
        public AudioClip defaultOpenSound;

        [Tooltip("Sound played when any panel closes")]
        public AudioClip defaultCloseSound;

        [Tooltip("Sound volume")]
        [Range(0f, 1f)]
        public float soundVolume = 1f;
    }

    /// <summary>
    /// How a panel is displayed on screen.
    /// </summary>
    public enum PanelDisplayMode
    {
        /// <summary>Panel fills the entire screen</summary>
        Fullscreen,

        /// <summary>Panel appears centered with configurable size</summary>
        Popup,

        /// <summary>Panel uses its own RectTransform anchors as-is</summary>
        Custom
    }

    /// <summary>
    /// Panel open/close animation types.
    /// </summary>
    public enum PanelAnimation
    {
        None,
        FadeIn,
        FadeOut,
        SlideFromTop,
        SlideFromBottom,
        SlideFromLeft,
        SlideFromRight,
        ScaleUp,
        ScaleDown
    }
}
