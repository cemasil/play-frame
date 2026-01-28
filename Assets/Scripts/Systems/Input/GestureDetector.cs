using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayFrame.Systems.Input
{
    /// <summary>
    /// Touch/Mouse gesture detector.
    /// Detects swipes, taps, long presses on UI elements.
    /// Works with both touch and mouse input.
    /// </summary>
    public class GestureDetector : IGestureDetector
    {
        public event Action<GestureData> OnGestureDetected;

        private readonly GestureSettings _settings;
        private readonly Camera _camera;

        private bool _isEnabled = true;
        private bool _isPressed;
        private Vector2 _startPosition;
        private float _startTime;
        private float _lastTapTime;
        private GameObject _currentTarget;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public GestureDetector(GestureSettings settings = null, Camera camera = null)
        {
            _settings = settings ?? GestureSettings.CreateDefault();
            _camera = camera ?? Camera.main;
        }

        public void UpdateDetection()
        {
            if (!_isEnabled) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnPressStart(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButton(0) && _isPressed)
            {
                OnPressHold(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && _isPressed)
            {
                OnPressEnd(UnityEngine.Input.mousePosition);
            }
        }

        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0) return;

            Touch touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnPressStart(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnPressHold(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnPressEnd(touch.position);
                    break;
            }
        }

        private void OnPressStart(Vector2 position)
        {
            _isPressed = true;
            _startPosition = position;
            _startTime = Time.time;
            _currentTarget = GetTargetUnderPosition(position);
        }

        private void OnPressHold(Vector2 position)
        {
            float duration = Time.time - _startTime;
            float distance = Vector2.Distance(_startPosition, position);

            // Check for long press
            if (duration >= _settings.LongPressTime && distance < _settings.DeadZone)
            {
                EmitGesture(GestureType.LongPress, _startPosition, position, duration);
                _isPressed = false;
            }
        }

        private void OnPressEnd(Vector2 position)
        {
            if (!_isPressed) return;

            float duration = Time.time - _startTime;
            float distance = Vector2.Distance(_startPosition, position);
            Vector2 delta = position - _startPosition;

            // Determine gesture type
            if (distance < _settings.TapThreshold && duration < _settings.MaxTapTime)
            {
                // Check for double tap
                if (Time.time - _lastTapTime < _settings.DoubleTapInterval)
                {
                    EmitGesture(GestureType.DoubleTap, _startPosition, position, duration);
                    _lastTapTime = 0;
                }
                else
                {
                    EmitGesture(GestureType.Tap, _startPosition, position, duration);
                    _lastTapTime = Time.time;
                }
            }
            else if (distance >= _settings.SwipeThreshold && duration < _settings.MaxSwipeTime)
            {
                // Swipe detected
                GestureType swipeType = GetSwipeType(delta);
                EmitGesture(swipeType, _startPosition, position, duration);
            }

            _isPressed = false;
            _currentTarget = null;
        }

        private GestureType GetSwipeType(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? GestureType.SwipeRight : GestureType.SwipeLeft;
            }
            else
            {
                return delta.y > 0 ? GestureType.SwipeUp : GestureType.SwipeDown;
            }
        }

        private void EmitGesture(GestureType type, Vector2 start, Vector2 end, float duration)
        {
            var gesture = GestureData.Create(type, start, end, duration, _currentTarget);
            OnGestureDetected?.Invoke(gesture);
        }

        private GameObject GetTargetUnderPosition(Vector2 screenPosition)
        {
            // Check UI elements first
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current?.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                return results[0].gameObject;
            }

            // Fallback to 2D/3D raycast
            if (_camera != null)
            {
                // 2D
                Vector2 worldPoint = _camera.ScreenToWorldPoint(screenPosition);
                RaycastHit2D hit2D = Physics2D.Raycast(worldPoint, Vector2.zero);
                if (hit2D.collider != null)
                {
                    return hit2D.collider.gameObject;
                }

                // 3D
                Ray ray = _camera.ScreenPointToRay(screenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit3D))
                {
                    return hit3D.collider.gameObject;
                }
            }

            return null;
        }
    }
}
