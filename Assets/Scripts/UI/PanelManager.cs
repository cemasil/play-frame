using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Core.Logging;
using PlayFrame.Systems.Audio;
using PlayFrame.UI.Base;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.UI
{
    /// <summary>
    /// Central panel manager with stack-based navigation, configurable animations,
    /// overlay, sound effects, and display mode support.
    /// </summary>
    public class PanelManager : PersistentSingleton<PanelManager>
    {
        private static readonly ILogger _logger = LoggerFactory.CreateUI("PanelManager");

        [Header("Panel References")]
        [Tooltip("Register all panels here via Inspector")]
        [SerializeField] private List<PanelEntry> registeredPanels = new List<PanelEntry>();

        [Header("Defaults")]
        [Tooltip("Global panel defaults (animation, sound, display). Create via PlayFrame/UI/Panel Defaults")]
        [SerializeField] private PanelDefaults defaults;

        [Header("Overlay")]
        [Tooltip("Overlay Image to darken background behind popup panels. Auto-created if null.")]
        [SerializeField] private Image overlayImage;

        [Header("Settings")]
        [Tooltip("Hide previous panel when showing a new one")]
        [SerializeField] private bool hidePreviousOnPush = false;

        // Panel stack for navigation
        private readonly Stack<UIPanel> _panelStack = new Stack<UIPanel>();
        private readonly Dictionary<string, UIPanel> _panelMap = new Dictionary<string, UIPanel>();
        private readonly Queue<PanelRequest> _panelQueue = new Queue<PanelRequest>();
        private bool _isTransitioning;
        private GameObject _overlayObject;

        /// <summary>Currently visible panel on top of the stack</summary>
        public UIPanel CurrentPanel => _panelStack.Count > 0 ? _panelStack.Peek() : null;

        /// <summary>Number of panels in the stack</summary>
        public int StackCount => _panelStack.Count;

        /// <summary>Is a transition currently in progress</summary>
        public bool IsTransitioning => _isTransitioning;

        /// <summary>Global panel defaults</summary>
        public PanelDefaults Defaults => defaults;

        protected override void OnSingletonAwake()
        {
            EnsureOverlay();
            InitializePanels();
        }

        private void EnsureOverlay()
        {
            if (overlayImage != null)
            {
                _overlayObject = overlayImage.gameObject;
                _overlayObject.SetActive(false);
                return;
            }

            // Auto-create overlay
            _overlayObject = new GameObject("PanelOverlay");
            _overlayObject.transform.SetParent(transform, false);
            _overlayObject.transform.SetAsFirstSibling();

            var rt = _overlayObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            overlayImage = _overlayObject.AddComponent<Image>();
            overlayImage.color = defaults != null ? defaults.overlayColor : new Color(0f, 0f, 0f, 0.6f);
            overlayImage.raycastTarget = true;

            var btn = _overlayObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(OnOverlayTapped);

            _overlayObject.SetActive(false);
        }

        private void OnOverlayTapped()
        {
            if (_panelStack.Count == 0) return;
            var current = _panelStack.Peek();
            if (current.CloseOnOverlayTap)
                HidePanel(current);
        }

        private void InitializePanels()
        {
            foreach (var entry in registeredPanels)
            {
                if (entry.panel != null && !string.IsNullOrEmpty(entry.panelId))
                {
                    _panelMap[entry.panelId] = entry.panel;
                }
            }
            _logger.Log($"PanelManager initialized with {_panelMap.Count} panels");
        }

        /// <summary>Register a panel at runtime</summary>
        public void RegisterPanel(string panelId, UIPanel panel)
        {
            if (panel == null || string.IsNullOrEmpty(panelId)) return;
            _panelMap[panelId] = panel;
        }

        /// <summary>Get a registered panel by ID</summary>
        public UIPanel GetPanel(string panelId)
        {
            _panelMap.TryGetValue(panelId, out var panel);
            return panel;
        }

        /// <summary>Get a typed panel by ID</summary>
        public T GetPanel<T>(string panelId) where T : UIPanel
        {
            return GetPanel(panelId) as T;
        }

        /// <summary>Show a panel by ID, pushing it onto the stack.</summary>
        public void ShowPanel(string panelId, Action onComplete = null)
        {
            if (!_panelMap.TryGetValue(panelId, out var panel))
            {
                _logger.LogWarning($"Panel not found: {panelId}");
                return;
            }
            ShowPanel(panel, onComplete);
        }

        /// <summary>Show a panel, pushing it onto the stack.</summary>
        public void ShowPanel(UIPanel panel, Action onComplete = null)
        {
            if (panel == null) return;

            if (_isTransitioning)
            {
                _panelQueue.Enqueue(new PanelRequest { Panel = panel, OnComplete = onComplete, IsShow = true });
                return;
            }

            ShowPanelAsync(panel, onComplete, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>Hide the top panel and pop from stack.</summary>
        public void HideCurrentPanel(Action onComplete = null)
        {
            if (_panelStack.Count == 0) return;
            var panel = _panelStack.Peek();
            HidePanel(panel, onComplete);
        }

        /// <summary>Hide a specific panel by ID.</summary>
        public void HidePanel(string panelId, Action onComplete = null)
        {
            if (!_panelMap.TryGetValue(panelId, out var panel))
            {
                _logger.LogWarning($"Panel not found: {panelId}");
                return;
            }
            HidePanel(panel, onComplete);
        }

        /// <summary>Hide a specific panel.</summary>
        public void HidePanel(UIPanel panel, Action onComplete = null)
        {
            if (panel == null) return;

            if (_isTransitioning)
            {
                _panelQueue.Enqueue(new PanelRequest { Panel = panel, OnComplete = onComplete, IsShow = false });
                return;
            }

            HidePanelAsync(panel, onComplete, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>Hide all panels and clear the stack.</summary>
        public void HideAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                var panel = _panelStack.Pop();
                panel.Hide();
                FirePanelEvent(panel, false);
            }
            UpdateOverlay();
        }

        /// <summary>Pop panels until reaching the target panel.</summary>
        public void PopTo(string panelId)
        {
            if (!_panelMap.TryGetValue(panelId, out var targetPanel)) return;

            while (_panelStack.Count > 0 && _panelStack.Peek() != targetPanel)
            {
                var panel = _panelStack.Pop();
                panel.Hide();
                FirePanelEvent(panel, false);
            }
            UpdateOverlay();
        }

        /// <summary>Show a sequence of panels one after another.</summary>
        public void ShowSequence(params string[] panelIds)
        {
            if (panelIds == null || panelIds.Length == 0) return;
            ShowSequenceInternal(panelIds, 0);
        }

        private void ShowSequenceInternal(string[] panelIds, int index)
        {
            if (index >= panelIds.Length) return;
            ShowPanel(panelIds[index]);
        }

        /// <summary>Show a panel and auto-hide after duration</summary>
        public void ShowPanelTimed(string panelId, float duration, Action onComplete = null)
        {
            ShowPanel(panelId, () =>
            {
                ShowTimedHideAsync(panelId, duration, onComplete, this.GetCancellationTokenOnDestroy()).Forget();
            });
        }

        private async UniTaskVoid ShowTimedHideAsync(string panelId, float duration, Action onComplete, CancellationToken ct)
        {
            try
            {
                await UniTask.WaitForSeconds(duration, ignoreTimeScale: true, cancellationToken: ct);
                HidePanel(panelId, onComplete);
            }
            catch (OperationCanceledException) { }
        }

        #region Internal Transitions

        private async UniTaskVoid ShowPanelAsync(UIPanel panel, Action onComplete, CancellationToken ct)
        {
            _isTransitioning = true;

            try
            {
                // Hide previous panel if configured
                if (hidePreviousOnPush && _panelStack.Count > 0)
                {
                    var previous = _panelStack.Peek();
                    await AnimateOutAsync(previous, ct);
                }

                // Apply display mode
                ApplyDisplayMode(panel);

                _panelStack.Push(panel);

                // Show overlay
                UpdateOverlay();

                // Bring panel to front (after overlay)
                panel.transform.SetAsLastSibling();

                // Play open sound
                PlaySound(panel.OpenSound, panel.SoundVolume);

                // Show panel (sets alpha=1, interactable=true)
                panel.Show();

                // Animate in
                await AnimateInAsync(panel, ct);

                FirePanelEvent(panel, true);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
            finally
            {
                _isTransitioning = false;
                ProcessQueue();
            }
        }

        private async UniTaskVoid HidePanelAsync(UIPanel panel, Action onComplete, CancellationToken ct)
        {
            _isTransitioning = true;

            try
            {
                // Play close sound
                PlaySound(panel.CloseSound, panel.SoundVolume);

                // Animate out
                await AnimateOutAsync(panel, ct);

                panel.Hide();

                // Remove from stack
                if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
                    _panelStack.Pop();

                // Update overlay
                UpdateOverlay();

                // Show previous panel if it was hidden
                if (hidePreviousOnPush && _panelStack.Count > 0)
                {
                    var previous = _panelStack.Peek();
                    previous.Show();
                    await AnimateInAsync(previous, ct);
                }

                FirePanelEvent(panel, false);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
            finally
            {
                _isTransitioning = false;
                ProcessQueue();
            }
        }

        #endregion

        #region Display Mode

        private void ApplyDisplayMode(UIPanel panel)
        {
            var rt = panel.GetComponent<RectTransform>();
            if (rt == null) return;

            switch (panel.DisplayMode)
            {
                case PanelDisplayMode.Fullscreen:
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    break;

                case PanelDisplayMode.Popup:
                    var size = panel.PopupSizeRatio;
                    float halfW = size.x * 0.5f;
                    float halfH = size.y * 0.5f;
                    rt.anchorMin = new Vector2(0.5f - halfW, 0.5f - halfH);
                    rt.anchorMax = new Vector2(0.5f + halfW, 0.5f + halfH);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    break;

                case PanelDisplayMode.Custom:
                    // Leave anchors as set in prefab
                    break;
            }
        }

        #endregion

        #region Animation

        private async UniTask AnimateInAsync(UIPanel panel, CancellationToken ct)
        {
            var anim = panel.OpenAnimation;
            float duration = panel.AnimationDuration;
            var curve = panel.AnimCurve;

            if (anim == PanelAnimation.None || duration <= 0f) return;

            var cg = panel.GetComponent<CanvasGroup>();
            var rt = panel.GetComponent<RectTransform>();

            switch (anim)
            {
                case PanelAnimation.FadeIn:
                    if (cg != null)
                        await FadeAsync(cg, 0f, 1f, duration, curve, ct);
                    break;

                case PanelAnimation.ScaleUp:
                    if (rt != null)
                    {
                        if (cg != null) cg.alpha = 1f;
                        await ScaleAsync(rt, Vector3.zero, Vector3.one, duration, curve, ct);
                    }
                    break;

                case PanelAnimation.SlideFromTop:
                    if (rt != null)
                        await SlideAsync(rt, new Vector2(0, Screen.height), Vector2.zero, duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromBottom:
                    if (rt != null)
                        await SlideAsync(rt, new Vector2(0, -Screen.height), Vector2.zero, duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromLeft:
                    if (rt != null)
                        await SlideAsync(rt, new Vector2(-Screen.width, 0), Vector2.zero, duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromRight:
                    if (rt != null)
                        await SlideAsync(rt, new Vector2(Screen.width, 0), Vector2.zero, duration, curve, ct);
                    break;
            }
        }

        private async UniTask AnimateOutAsync(UIPanel panel, CancellationToken ct)
        {
            var anim = panel.CloseAnimation;
            float duration = panel.AnimationDuration;
            var curve = panel.AnimCurve;

            if (anim == PanelAnimation.None || duration <= 0f) return;

            var cg = panel.GetComponent<CanvasGroup>();
            var rt = panel.GetComponent<RectTransform>();

            switch (anim)
            {
                case PanelAnimation.FadeOut:
                    if (cg != null)
                        await FadeAsync(cg, 1f, 0f, duration, curve, ct);
                    break;

                case PanelAnimation.ScaleDown:
                    if (rt != null)
                        await ScaleAsync(rt, Vector3.one, Vector3.zero, duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromTop:
                    if (rt != null)
                        await SlideAsync(rt, Vector2.zero, new Vector2(0, Screen.height), duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromBottom:
                    if (rt != null)
                        await SlideAsync(rt, Vector2.zero, new Vector2(0, -Screen.height), duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromLeft:
                    if (rt != null)
                        await SlideAsync(rt, Vector2.zero, new Vector2(-Screen.width, 0), duration, curve, ct);
                    break;

                case PanelAnimation.SlideFromRight:
                    if (rt != null)
                        await SlideAsync(rt, Vector2.zero, new Vector2(Screen.width, 0), duration, curve, ct);
                    break;
            }
        }

        private async UniTask FadeAsync(CanvasGroup cg, float from, float to, float duration, AnimationCurve curve, CancellationToken ct)
        {
            float elapsed = 0f;
            cg.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                cg.alpha = Mathf.Lerp(from, to, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            cg.alpha = to;
        }

        private async UniTask ScaleAsync(RectTransform rt, Vector3 from, Vector3 to, float duration, AnimationCurve curve, CancellationToken ct)
        {
            float elapsed = 0f;
            rt.localScale = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                rt.localScale = Vector3.LerpUnclamped(from, to, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            rt.localScale = to;
        }

        private async UniTask SlideAsync(RectTransform rt, Vector2 from, Vector2 to, float duration, AnimationCurve curve, CancellationToken ct)
        {
            float elapsed = 0f;
            rt.anchoredPosition = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            rt.anchoredPosition = to;
        }

        #endregion

        #region Overlay

        private void UpdateOverlay()
        {
            if (_overlayObject == null) return;

            // Find topmost panel that wants overlay
            UIPanel topOverlayPanel = null;
            foreach (var panel in _panelStack)
            {
                if (panel.ShowOverlay && panel.DisplayMode != PanelDisplayMode.Fullscreen)
                {
                    topOverlayPanel = panel;
                    break; // Stack iterates top-first
                }
            }

            if (topOverlayPanel != null)
            {
                overlayImage.color = topOverlayPanel.OverlayColor;
                _overlayObject.SetActive(true);
                // Place overlay just before the topmost overlay panel
                _overlayObject.transform.SetSiblingIndex(topOverlayPanel.transform.GetSiblingIndex());
            }
            else
            {
                _overlayObject.SetActive(false);
            }
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip, float volume)
        {
            if (clip == null) return;
            if (AudioManager.HasInstance)
                AudioManager.Instance.PlaySFX(clip, volume);
        }

        #endregion

        private void ProcessQueue()
        {
            if (_panelQueue.Count > 0 && !_isTransitioning)
            {
                var request = _panelQueue.Dequeue();
                if (request.IsShow)
                    ShowPanel(request.Panel, request.OnComplete);
                else
                    HidePanel(request.Panel, request.OnComplete);
            }
        }

        private void FirePanelEvent(UIPanel panel, bool opened)
        {
            if (!EventManager.HasInstance) return;

            string panelName = panel.GetType().Name;
            if (opened)
                EventManager.Instance.TriggerEvent(CoreEvents.UIPanelOpened, panelName);
            else
                EventManager.Instance.TriggerEvent(CoreEvents.UIPanelClosed, panelName);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < registeredPanels.Count; i++)
            {
                var entry = registeredPanels[i];
                if (entry.panel != null && string.IsNullOrEmpty(entry.panelId))
                {
                    string typeName = entry.panel.GetType().Name;
                    if (typeName.EndsWith("Panel"))
                        typeName = typeName.Substring(0, typeName.Length - 5);
                    entry.panelId = typeName;
                    registeredPanels[i] = entry;
                }
            }
        }
#endif

        [Serializable]
        private struct PanelEntry
        {
            public string panelId;
            public UIPanel panel;
        }

        private struct PanelRequest
        {
            public UIPanel Panel;
            public Action OnComplete;
            public bool IsShow;
        }
    }
}
