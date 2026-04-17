using UnityEngine;

namespace PlayFrame.UI.Base
{
    /// <summary>
    /// Base class for all UI panels.
    /// Each panel can override display mode, animation, and sound from the global PanelDefaults.
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool hideOnAwake = true;

        [Header("Display Override")]
        [Tooltip("Override the default display mode for this panel")]
        [SerializeField] private bool overrideDisplayMode = false;
        [SerializeField] private PanelDisplayMode displayMode = PanelDisplayMode.Popup;

        [Tooltip("Popup size ratio (only for Popup mode)")]
        [SerializeField] private Vector2 popupSizeRatio = new Vector2(0.85f, 0.7f);

        [Header("Animation Override")]
        [Tooltip("Override the default animation for this panel")]
        [SerializeField] private bool overrideAnimation = false;
        [SerializeField] private PanelAnimation openAnimation = PanelAnimation.FadeIn;
        [SerializeField] private PanelAnimation closeAnimation = PanelAnimation.FadeOut;
        [SerializeField][Range(0f, 1f)] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Overlay Override")]
        [Tooltip("Override the default overlay setting for this panel")]
        [SerializeField] private bool overrideOverlay = false;
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private bool closeOnOverlayTap = false;

        [Header("Audio Override")]
        [Tooltip("Override the default sounds for this panel")]
        [SerializeField] private bool overrideAudio = false;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        protected bool isVisible = false;

        public bool IsVisible => isVisible;

        // Resolved settings (panel override > global defaults)
        public PanelDisplayMode DisplayMode =>
            overrideDisplayMode ? displayMode : (GetDefaults()?.displayMode ?? PanelDisplayMode.Popup);

        public Vector2 PopupSizeRatio =>
            overrideDisplayMode ? popupSizeRatio :
            new Vector2(GetDefaults()?.popupWidthRatio ?? 0.85f, GetDefaults()?.popupHeightRatio ?? 0.7f);

        public PanelAnimation OpenAnimation =>
            overrideAnimation ? openAnimation : (GetDefaults()?.openAnimation ?? PanelAnimation.FadeIn);

        public PanelAnimation CloseAnimation =>
            overrideAnimation ? closeAnimation : (GetDefaults()?.closeAnimation ?? PanelAnimation.FadeOut);

        public float AnimationDuration =>
            overrideAnimation ? animationDuration : (GetDefaults()?.animationDuration ?? 0.3f);

        public AnimationCurve AnimCurve =>
            overrideAnimation ? animationCurve : (GetDefaults()?.animationCurve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

        public bool ShowOverlay =>
            overrideOverlay ? showOverlay : (GetDefaults()?.showOverlay ?? true);

        public Color OverlayColor =>
            overrideOverlay ? overlayColor : (GetDefaults()?.overlayColor ?? new Color(0f, 0f, 0f, 0.6f));

        public bool CloseOnOverlayTap =>
            overrideOverlay ? closeOnOverlayTap : (GetDefaults()?.closeOnOverlayTap ?? false);

        public AudioClip OpenSound =>
            overrideAudio ? openSound : GetDefaults()?.defaultOpenSound;

        public AudioClip CloseSound =>
            overrideAudio ? closeSound : GetDefaults()?.defaultCloseSound;

        public float SoundVolume => GetDefaults()?.soundVolume ?? 1f;

        private PanelDefaults _cachedDefaults;

        private PanelDefaults GetDefaults()
        {
            if (_cachedDefaults == null && PanelManager.HasInstance)
                _cachedDefaults = PanelManager.Instance.Defaults;
            return _cachedDefaults;
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            Initialize();

            if (hideOnAwake)
            {
                Hide();
            }
        }

        public virtual void Initialize()
        {
            OnInitialize();
        }
        public void Update()
        {
            OnUpdate();
        }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isVisible = true;

            OnShow();
        }
        public virtual void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isVisible = false;

            OnHide();
        }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnCleanup() { }
        public virtual void UpdatePanel() { }
        protected virtual void OnDestroy()
        {
            OnCleanup();
        }
    }
}
