using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniGameFramework.Systems.Input
{
    /// <summary>
    /// Handles tap/click input for UI elements.
    /// Attach to any UI element that needs tap detection.
    /// Implements ITappable interface for consistency.
    /// </summary>
    public class TapHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float doubleTapThreshold = 0.3f;
        [SerializeField] private bool enableDoubleTap = false;

        public event System.Action OnTap;
        public event System.Action OnDoubleTap;
        public event System.Action OnPointerDownEvent;
        public event System.Action OnPointerUpEvent;

        private float _lastTapTime;
        private ITappable _tappable;

        private void Awake()
        {
            _tappable = GetComponent<ITappable>();
        }

        /// <summary>
        /// Register a callback for tap events
        /// </summary>
        public void OnTapCallback(System.Action callback)
        {
            OnTap += callback;
        }

        /// <summary>
        /// Unregister a callback for tap events
        /// </summary>
        public void RemoveTapCallback(System.Action callback)
        {
            OnTap -= callback;
        }

        /// <summary>
        /// Register a callback for double tap events
        /// </summary>
        public void OnDoubleTapCallback(System.Action callback)
        {
            enableDoubleTap = true;
            OnDoubleTap += callback;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (enableDoubleTap)
            {
                float timeSinceLastTap = Time.unscaledTime - _lastTapTime;

                if (timeSinceLastTap <= doubleTapThreshold)
                {
                    HandleDoubleTap();
                    _lastTapTime = 0f;
                }
                else
                {
                    HandleTap();
                    _lastTapTime = Time.unscaledTime;
                }
            }
            else
            {
                HandleTap();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke();
        }

        private void HandleTap()
        {
            OnTap?.Invoke();
            _tappable?.OnTap();
        }

        private void HandleDoubleTap()
        {
            OnDoubleTap?.Invoke();
        }

        private void OnDestroy()
        {
            OnTap = null;
            OnDoubleTap = null;
            OnPointerDownEvent = null;
            OnPointerUpEvent = null;
        }
    }
}
