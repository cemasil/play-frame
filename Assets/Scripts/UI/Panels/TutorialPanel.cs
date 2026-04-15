using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Step-by-step tutorial panel for onboarding or level-specific tutorials.
    /// Supports sequential steps with text, images, and optional highlight targets.
    /// </summary>
    public class TutorialPanel : UIPanel
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image tutorialImage;
        [SerializeField] private TextMeshProUGUI stepCounterText;

        [Header("Highlight")]
        [SerializeField] private RectTransform highlightOverlay;
        [SerializeField] private RectTransform highlightCutout;

        [Header("Hand Pointer")]
        [SerializeField] private RectTransform handPointer;

        [Header("Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button doneButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;

        private List<TutorialStep> _steps = new List<TutorialStep>();
        private int _currentStepIndex;

        /// <summary>Called when tutorial is completed (all steps viewed)</summary>
        public event Action OnTutorialComplete;

        /// <summary>Called when tutorial is skipped</summary>
        public event Action OnTutorialSkipped;

        /// <summary>Called when step changes (step index)</summary>
        public event Action<int> OnStepChanged;

        public int CurrentStep => _currentStepIndex;
        public int TotalSteps => _steps.Count;

        protected override void OnInitialize()
        {
            if (nextButton != null) nextButton.onClick.AddListener(NextStep);
            if (prevButton != null) prevButton.onClick.AddListener(PrevStep);
            if (skipButton != null) skipButton.onClick.AddListener(Skip);
            if (doneButton != null) doneButton.onClick.AddListener(Complete);
        }

        /// <summary>
        /// Set up tutorial with steps. Show the panel after calling this.
        /// </summary>
        public void Setup(List<TutorialStep> steps)
        {
            _steps = steps ?? new List<TutorialStep>();
            _currentStepIndex = 0;
        }

        protected override void OnShow()
        {
            _currentStepIndex = 0;
            ShowCurrentStep();
        }

        /// <summary>
        /// Navigate to next step.
        /// </summary>
        public void NextStep()
        {
            if (_currentStepIndex < _steps.Count - 1)
            {
                _currentStepIndex++;
                ShowCurrentStep();
            }
            else
            {
                Complete();
            }
        }

        /// <summary>
        /// Navigate to previous step.
        /// </summary>
        public void PrevStep()
        {
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                ShowCurrentStep();
            }
        }

        private void ShowCurrentStep()
        {
            if (_steps.Count == 0) return;
            if (_currentStepIndex < 0 || _currentStepIndex >= _steps.Count) return;

            var step = _steps[_currentStepIndex];

            if (titleText != null) titleText.text = step.Title;
            if (descriptionText != null) descriptionText.text = step.Description;

            if (tutorialImage != null)
            {
                bool hasImage = step.Image != null;
                tutorialImage.gameObject.SetActive(hasImage);
                if (hasImage) tutorialImage.sprite = step.Image;
            }

            if (stepCounterText != null)
                stepCounterText.text = $"{_currentStepIndex + 1} / {_steps.Count}";

            // Navigation buttons
            bool isFirst = _currentStepIndex == 0;
            bool isLast = _currentStepIndex == _steps.Count - 1;

            if (prevButton != null) prevButton.gameObject.SetActive(!isFirst);
            if (nextButton != null) nextButton.gameObject.SetActive(!isLast);
            if (doneButton != null) doneButton.gameObject.SetActive(isLast);
            if (nextButtonText != null) nextButtonText.text = "Next";

            // Highlight target
            UpdateHighlight(step.HighlightTarget, step.HighlightPadding);

            // Hand pointer
            UpdateHandPointer(step.HandPointerTarget, step.HandPointerOffset);

            OnStepChanged?.Invoke(_currentStepIndex);
        }

        private void UpdateHighlight(RectTransform target, Vector2 padding)
        {
            if (highlightOverlay == null) return;

            if (target == null)
            {
                highlightOverlay.gameObject.SetActive(false);
                return;
            }

            highlightOverlay.gameObject.SetActive(true);

            if (highlightCutout != null)
            {
                highlightCutout.position = target.position;
                highlightCutout.sizeDelta = target.sizeDelta + padding;
            }
        }

        private void UpdateHandPointer(RectTransform target, Vector2 offset)
        {
            if (handPointer == null) return;

            if (target == null)
            {
                handPointer.gameObject.SetActive(false);
                return;
            }

            handPointer.gameObject.SetActive(true);
            handPointer.position = target.position + (Vector3)offset;
        }

        private void Skip()
        {
            OnTutorialSkipped?.Invoke();
            CloseTutorial();
        }

        private void Complete()
        {
            OnTutorialComplete?.Invoke();
            CloseTutorial();
        }

        private void CloseTutorial()
        {
            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);
        }

        protected override void OnCleanup()
        {
            if (nextButton != null) nextButton.onClick.RemoveAllListeners();
            if (prevButton != null) prevButton.onClick.RemoveAllListeners();
            if (skipButton != null) skipButton.onClick.RemoveAllListeners();
            if (doneButton != null) doneButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Data for a single tutorial step.
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        public string Title;
        public string Description;
        public Sprite Image;
        public RectTransform HighlightTarget;
        public Vector2 HighlightPadding = new Vector2(20f, 20f);
        public RectTransform HandPointerTarget;
        public Vector2 HandPointerOffset;
    }
}
