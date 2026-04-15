using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Systems.Audio;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Pause panel shown during gameplay.
    /// Supports resume, restart, settings, and quit actions.
    /// </summary>
    public class PausePanel : UIPanel
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button quitButton;

        [Header("Quick Audio Toggles")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;

        /// <summary>Called when Resume is tapped</summary>
        public event Action OnResume;

        /// <summary>Called when Restart is tapped</summary>
        public event Action OnRestart;

        /// <summary>Called when Settings is tapped</summary>
        public event Action OnSettings;

        /// <summary>Called when Home is tapped</summary>
        public event Action OnHome;

        /// <summary>Called when Quit is tapped</summary>
        public event Action OnQuit;

        protected override void OnInitialize()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(() => OnRestart?.Invoke());
            if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettings?.Invoke());
            if (homeButton != null) homeButton.onClick.AddListener(() => OnHome?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuit?.Invoke());

            if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggled);
            if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxToggled);
        }

        /// <summary>
        /// Set level info displayed in the pause screen.
        /// </summary>
        public void Setup(string title = "Paused", string level = null)
        {
            if (titleText != null) titleText.text = title;
            if (levelText != null)
            {
                levelText.gameObject.SetActive(!string.IsNullOrEmpty(level));
                levelText.text = level;
            }
        }

        protected override void OnShow()
        {
            Time.timeScale = 0f;

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(CoreEvents.GamePaused);

            RefreshAudioToggles();
        }

        protected override void OnHide()
        {
            Time.timeScale = 1f;

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(CoreEvents.GameResumed);
        }

        private void Resume()
        {
            OnResume?.Invoke();

            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);
        }

        private void RefreshAudioToggles()
        {
            if (!AudioManager.HasInstance) return;

            if (musicToggle != null)
                musicToggle.SetIsOnWithoutNotify(!AudioManager.Instance.IsMusicMuted);

            if (sfxToggle != null)
                sfxToggle.SetIsOnWithoutNotify(!AudioManager.Instance.IsSfxMuted);
        }

        private void OnMusicToggled(bool isOn)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetMusicMuted(!isOn);
        }

        private void OnSfxToggled(bool isOn)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetSfxMuted(!isOn);
        }

        protected override void OnCleanup()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
            if (restartButton != null) restartButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (homeButton != null) homeButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
            if (musicToggle != null) musicToggle.onValueChanged.RemoveAllListeners();
            if (sfxToggle != null) sfxToggle.onValueChanged.RemoveAllListeners();
        }
    }
}
