using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Core.Logging;
using PlayFrame.UI.Base;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.UI
{
    /// <summary>
    /// Central panel manager that handles showing/hiding panels with a stack-based system.
    /// Supports panel queues, transitions, and flow control.
    /// Add this to the Canvas or a persistent UI manager object.
    /// </summary>
    public class PanelManager : PersistentSingleton<PanelManager>
    {
        private static readonly ILogger _logger = LoggerFactory.CreateUI("PanelManager");

        [Header("Panel References")]
        [Tooltip("Register all panels here via Inspector")]
        [SerializeField] private List<PanelEntry> registeredPanels = new List<PanelEntry>();

        [Header("Settings")]
        [Tooltip("Duration for panel show/hide transitions")]
        [SerializeField] private float transitionDuration = 0.3f;

        [Tooltip("Hide previous panel when showing a new one")]
        [SerializeField] private bool hidePreviousOnPush = false;

        // Panel stack for navigation
        private readonly Stack<UIPanel> _panelStack = new Stack<UIPanel>();
        private readonly Dictionary<string, UIPanel> _panelMap = new Dictionary<string, UIPanel>();
        private readonly Queue<PanelRequest> _panelQueue = new Queue<PanelRequest>();
        private bool _isTransitioning;

        /// <summary>Currently visible panel on top of the stack</summary>
        public UIPanel CurrentPanel => _panelStack.Count > 0 ? _panelStack.Peek() : null;

        /// <summary>Number of panels in the stack</summary>
        public int StackCount => _panelStack.Count;

        /// <summary>Is a transition currently in progress</summary>
        public bool IsTransitioning => _isTransitioning;

        protected override void OnSingletonAwake()
        {
            InitializePanels();
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

        /// <summary>
        /// Register a panel at runtime
        /// </summary>
        public void RegisterPanel(string panelId, UIPanel panel)
        {
            if (panel == null || string.IsNullOrEmpty(panelId)) return;
            _panelMap[panelId] = panel;
        }

        /// <summary>
        /// Get a registered panel by ID
        /// </summary>
        public UIPanel GetPanel(string panelId)
        {
            _panelMap.TryGetValue(panelId, out var panel);
            return panel;
        }

        /// <summary>
        /// Get a typed panel by ID
        /// </summary>
        public T GetPanel<T>(string panelId) where T : UIPanel
        {
            return GetPanel(panelId) as T;
        }

        /// <summary>
        /// Show a panel by ID, pushing it onto the stack.
        /// </summary>
        public void ShowPanel(string panelId, Action onComplete = null)
        {
            if (!_panelMap.TryGetValue(panelId, out var panel))
            {
                _logger.LogWarning($"Panel not found: {panelId}");
                return;
            }

            ShowPanel(panel, onComplete);
        }

        /// <summary>
        /// Show a panel, pushing it onto the stack.
        /// </summary>
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

        /// <summary>
        /// Hide the top panel and pop from stack.
        /// </summary>
        public void HideCurrentPanel(Action onComplete = null)
        {
            if (_panelStack.Count == 0) return;
            var panel = _panelStack.Peek();
            HidePanel(panel, onComplete);
        }

        /// <summary>
        /// Hide a specific panel.
        /// </summary>
        public void HidePanel(string panelId, Action onComplete = null)
        {
            if (!_panelMap.TryGetValue(panelId, out var panel))
            {
                _logger.LogWarning($"Panel not found: {panelId}");
                return;
            }

            HidePanel(panel, onComplete);
        }

        /// <summary>
        /// Hide a specific panel.
        /// </summary>
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

        /// <summary>
        /// Hide all panels and clear the stack.
        /// </summary>
        public void HideAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                var panel = _panelStack.Pop();
                panel.Hide();
                FirePanelEvent(panel, false);
            }
        }

        /// <summary>
        /// Pop panels until reaching the target panel.
        /// </summary>
        public void PopTo(string panelId)
        {
            if (!_panelMap.TryGetValue(panelId, out var targetPanel)) return;

            while (_panelStack.Count > 0 && _panelStack.Peek() != targetPanel)
            {
                var panel = _panelStack.Pop();
                panel.Hide();
                FirePanelEvent(panel, false);
            }
        }

        /// <summary>
        /// Show a sequence of panels one after another.
        /// Each panel is shown when onComplete is called from the previous.
        /// </summary>
        public void ShowSequence(params string[] panelIds)
        {
            if (panelIds == null || panelIds.Length == 0) return;

            ShowSequenceInternal(panelIds, 0);
        }

        private void ShowSequenceInternal(string[] panelIds, int index)
        {
            if (index >= panelIds.Length) return;

            ShowPanel(panelIds[index], () =>
            {
                // Wait for panel to be hidden before showing next
                // The panel itself should call HideCurrentPanel when done
            });
        }

        /// <summary>
        /// Show a panel and auto-hide after duration
        /// </summary>
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
                await UniTask.WaitForSeconds(duration, cancellationToken: ct);
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
                    await TransitionOutAsync(previous, ct);
                }

                _panelStack.Push(panel);
                panel.Show();
                await TransitionInAsync(panel, ct);

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
                await TransitionOutAsync(panel, ct);
                panel.Hide();

                // Remove from stack if it's there
                if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
                {
                    _panelStack.Pop();
                }

                // Show previous panel if it was hidden
                if (hidePreviousOnPush && _panelStack.Count > 0)
                {
                    var previous = _panelStack.Peek();
                    previous.Show();
                    await TransitionInAsync(previous, ct);
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

        private async UniTask TransitionInAsync(UIPanel panel, CancellationToken ct)
        {
            if (transitionDuration <= 0f) return;

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) return;

            float elapsed = 0f;
            cg.alpha = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / transitionDuration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            cg.alpha = 1f;
        }

        private async UniTask TransitionOutAsync(UIPanel panel, CancellationToken ct)
        {
            if (transitionDuration <= 0f) return;

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) return;

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = 1f - Mathf.Clamp01(elapsed / transitionDuration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            cg.alpha = 0f;
        }

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

        #endregion

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
