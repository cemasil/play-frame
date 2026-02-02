using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.Core.Events;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Loading Panel - Listens to scene loading events and displays progress
    /// </summary>
    public class LoadingPanel : UIPanel
    {
        [Header("Loading UI")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float animationSpeed = 50f;

        private float pulseTimer;
        private float targetProgress = 0f;
        private float currentProgress = 0f;

        protected override void OnInitialize()
        {
            EventManager.Instance.Subscribe(CoreEvents.SceneLoadStarted, OnSceneLoadStarted);
            EventManager.Instance.Subscribe(CoreEvents.SceneLoadProgress, OnSceneLoadProgress);
            EventManager.Instance.Subscribe(CoreEvents.SceneLoadCompleted, OnSceneLoadCompleted);
        }

        protected override void OnShow()
        {
            currentProgress = 0f;
            targetProgress = 0f;
        }

        protected override void OnUpdate()
        {
            if (currentProgress < targetProgress)
            {
                currentProgress += Time.deltaTime * animationSpeed;
                currentProgress = Mathf.Min(currentProgress, targetProgress);
            }

            UpdateUI();
        }

        private void OnSceneLoadStarted()
        {
            Show();
            currentProgress = 0f;
            targetProgress = 0f;
        }

        private void OnSceneLoadProgress(float progress)
        {
            targetProgress = progress * 100f;
        }

        private void OnSceneLoadCompleted()
        {
            targetProgress = 100f;
            StartCoroutine(HideAfterDelay(0.5f));
        }

        private System.Collections.IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide();
        }

        public void SetProgress(float progress)
        {
            targetProgress = Mathf.Clamp(progress, 0f, 100f);
        }

        private void UpdateUI()
        {
            if (percentageText != null)
                percentageText.text = $"{Mathf.RoundToInt(currentProgress)}%";

            if (loadingText != null)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float scale = 1f + Mathf.Sin(pulseTimer) * 0.1f;
                loadingText.transform.localScale = Vector3.one * scale;
            }
        }

        protected override void OnCleanup()
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.Unsubscribe(CoreEvents.SceneLoadStarted, OnSceneLoadStarted);
                EventManager.Instance.Unsubscribe(CoreEvents.SceneLoadProgress, OnSceneLoadProgress);
                EventManager.Instance.Unsubscribe(CoreEvents.SceneLoadCompleted, OnSceneLoadCompleted);
            }
        }
    }
}
