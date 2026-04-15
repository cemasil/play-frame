using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Systems.Audio;
using PlayFrame.Systems.Save;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Settings panel with audio controls, language selection, and general options.
    /// Integrates with AudioManager and SaveManager for persistence.
    /// </summary>
    public class SettingsPanel : UIPanel
    {
        [Header("Audio Controls")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle musicMuteToggle;
        [SerializeField] private Toggle sfxMuteToggle;

        [Header("Language")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private string[] languageCodes = { "en", "tr", "de", "fr", "es", "pt", "ja", "ko", "zh" };
        [SerializeField] private string[] languageNames = { "English", "Türkçe", "Deutsch", "Français", "Español", "Português", "日本語", "한국어", "中文" };

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button restorePurchasesButton;

        [Header("Haptic")]
        [SerializeField] private Toggle hapticToggle;

        /// <summary>Called when restore purchases is requested</summary>
        public event Action OnRestorePurchasesRequested;

        /// <summary>Called when delete save is confirmed</summary>
        public event Action OnDeleteSaveConfirmed;

        protected override void OnInitialize()
        {
            SetupSliders();
            SetupToggles();
            SetupLanguageDropdown();
            SetupButtons();
        }

        private void SetupSliders()
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
        }

        private void SetupToggles()
        {
            if (musicMuteToggle != null)
                musicMuteToggle.onValueChanged.AddListener(OnMusicMuteToggled);

            if (sfxMuteToggle != null)
                sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteToggled);

            if (hapticToggle != null)
                hapticToggle.onValueChanged.AddListener(OnHapticToggled);
        }

        private void SetupLanguageDropdown()
        {
            if (languageDropdown == null) return;

            languageDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>(languageNames);
            languageDropdown.AddOptions(options);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        private void SetupButtons()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            if (deleteSaveButton != null)
                deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);

            if (restorePurchasesButton != null)
                restorePurchasesButton.onClick.AddListener(OnRestorePurchasesClicked);
        }

        protected override void OnShow()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (AudioManager.HasInstance)
            {
                if (musicVolumeSlider != null)
                    musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);

                if (sfxVolumeSlider != null)
                    sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);

                if (musicMuteToggle != null)
                    musicMuteToggle.SetIsOnWithoutNotify(!AudioManager.Instance.IsMusicMuted);

                if (sfxMuteToggle != null)
                    sfxMuteToggle.SetIsOnWithoutNotify(!AudioManager.Instance.IsSfxMuted);
            }
        }

        #region Audio Callbacks

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetSfxVolume(value);
        }

        private void OnMusicMuteToggled(bool isOn)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetMusicMuted(!isOn);
        }

        private void OnSfxMuteToggled(bool isOn)
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.SetSfxMuted(!isOn);
        }

        private void OnHapticToggled(bool isOn)
        {
            if (SaveManager.HasInstance)
            {
                // Store haptic preference in save data
                SaveManager.Instance.CurrentSaveData.SetCustomData("haptic_enabled", isOn ? "1" : "0");
                SaveManager.Instance.SaveGame();
            }
        }

        #endregion

        #region Language

        private void OnLanguageChanged(int index)
        {
            if (index < 0 || index >= languageCodes.Length) return;

            string langCode = languageCodes[index];

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(CoreEvents.LanguageChanged, langCode);
        }

        /// <summary>
        /// Set the language dropdown to a specific code (call from localization system)
        /// </summary>
        public void SetLanguage(string code)
        {
            if (languageDropdown == null) return;

            int index = Array.IndexOf(languageCodes, code);
            if (index >= 0)
                languageDropdown.SetValueWithoutNotify(index);
        }

        #endregion

        #region Button Callbacks

        private void OnCloseClicked()
        {
            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);
        }

        private void OnResetClicked()
        {
            if (AudioManager.HasInstance)
                AudioManager.Instance.ResetToDefaults();

            RefreshUI();
        }

        private void OnDeleteSaveClicked()
        {
            OnDeleteSaveConfirmed?.Invoke();
        }

        private void OnRestorePurchasesClicked()
        {
            OnRestorePurchasesRequested?.Invoke();
        }

        #endregion

        protected override void OnCleanup()
        {
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            if (musicMuteToggle != null) musicMuteToggle.onValueChanged.RemoveAllListeners();
            if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.RemoveAllListeners();
            if (hapticToggle != null) hapticToggle.onValueChanged.RemoveAllListeners();
            if (languageDropdown != null) languageDropdown.onValueChanged.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (resetButton != null) resetButton.onClick.RemoveAllListeners();
            if (deleteSaveButton != null) deleteSaveButton.onClick.RemoveAllListeners();
            if (restorePurchasesButton != null) restorePurchasesButton.onClick.RemoveAllListeners();
        }
    }
}
