using UnityEngine;
using UnityEngine.UI;

namespace MiniGameFramework.Systems.Audio
{
    /// <summary>
    /// UI component for audio settings. Attach to settings panel.
    /// </summary>
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Music Controls")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Button musicMuteButton;

        [Header("SFX Controls")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Button sfxMuteButton;

        [Header("Icons (Optional)")]
        [SerializeField] private Image musicIcon;
        [SerializeField] private Image sfxIcon;
        [SerializeField] private Sprite musicOnSprite;
        [SerializeField] private Sprite musicOffSprite;
        [SerializeField] private Sprite sfxOnSprite;
        [SerializeField] private Sprite sfxOffSprite;

        [Header("Test Sound")]
        [SerializeField] private AudioClip testSfxClip;

        private void Start()
        {
            InitializeUI();
            RegisterCallbacks();
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }

        private void InitializeUI()
        {
            if (!AudioManager.HasInstance) return;

            // Set initial slider values
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.MusicVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.SfxVolume;
            }

            // Set initial toggle values
            if (musicToggle != null)
            {
                musicToggle.isOn = !AudioManager.Instance.IsMusicMuted;
            }

            if (sfxToggle != null)
            {
                sfxToggle.isOn = !AudioManager.Instance.IsSfxMuted;
            }

            UpdateMusicIcon();
            UpdateSfxIcon();
        }

        private void RegisterCallbacks()
        {
            if (musicSlider != null)
                musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

            if (musicToggle != null)
                musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);

            if (sfxToggle != null)
                sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);

            if (musicMuteButton != null)
                musicMuteButton.onClick.AddListener(OnMusicMuteClicked);

            if (sfxMuteButton != null)
                sfxMuteButton.onClick.AddListener(OnSfxMuteClicked);

            // Subscribe to AudioManager events
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.OnMusicVolumeChanged += OnMusicVolumeChanged;
                AudioManager.Instance.OnSfxVolumeChanged += OnSfxVolumeChanged;
            }
        }

        private void UnregisterCallbacks()
        {
            if (musicSlider != null)
                musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

            if (sfxSlider != null)
                sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

            if (musicToggle != null)
                musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);

            if (sfxToggle != null)
                sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChanged);

            if (musicMuteButton != null)
                musicMuteButton.onClick.RemoveListener(OnMusicMuteClicked);

            if (sfxMuteButton != null)
                sfxMuteButton.onClick.RemoveListener(OnSfxMuteClicked);

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.OnMusicVolumeChanged -= OnMusicVolumeChanged;
                AudioManager.Instance.OnSfxVolumeChanged -= OnSfxVolumeChanged;
            }
        }

        #region Slider Callbacks

        private void OnMusicSliderChanged(float value)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.MusicVolume = value;
            }
        }

        private void OnSfxSliderChanged(float value)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.SfxVolume = value;

                // Play test sound when adjusting
                if (testSfxClip != null)
                {
                    AudioManager.Instance.PlaySFX(testSfxClip);
                }
            }
        }

        #endregion

        #region Toggle Callbacks

        private void OnMusicToggleChanged(bool isOn)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.IsMusicMuted = !isOn;
                UpdateMusicIcon();
            }
        }

        private void OnSfxToggleChanged(bool isOn)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.IsSfxMuted = !isOn;
                UpdateSfxIcon();
            }
        }

        #endregion

        #region Button Callbacks

        private void OnMusicMuteClicked()
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.IsMusicMuted = !AudioManager.Instance.IsMusicMuted;
                
                if (musicToggle != null)
                    musicToggle.isOn = !AudioManager.Instance.IsMusicMuted;
                
                UpdateMusicIcon();
            }
        }

        private void OnSfxMuteClicked()
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.IsSfxMuted = !AudioManager.Instance.IsSfxMuted;
                
                if (sfxToggle != null)
                    sfxToggle.isOn = !AudioManager.Instance.IsSfxMuted;
                
                UpdateSfxIcon();
            }
        }

        #endregion

        #region AudioManager Event Handlers

        private void OnMusicVolumeChanged(float volume)
        {
            if (musicSlider != null && !Mathf.Approximately(musicSlider.value, volume))
            {
                musicSlider.value = volume;
            }
        }

        private void OnSfxVolumeChanged(float volume)
        {
            if (sfxSlider != null && !Mathf.Approximately(sfxSlider.value, volume))
            {
                sfxSlider.value = volume;
            }
        }

        #endregion

        #region Icon Updates

        private void UpdateMusicIcon()
        {
            if (musicIcon == null) return;

            bool isMuted = AudioManager.HasInstance && AudioManager.Instance.IsMusicMuted;
            musicIcon.sprite = isMuted ? musicOffSprite : musicOnSprite;
        }

        private void UpdateSfxIcon()
        {
            if (sfxIcon == null) return;

            bool isMuted = AudioManager.HasInstance && AudioManager.Instance.IsSfxMuted;
            sfxIcon.sprite = isMuted ? sfxOffSprite : sfxOnSprite;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset audio settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.ResetToDefaults();
                InitializeUI();
            }
        }

        #endregion
    }
}
