using System;
using UnityEngine;
using PlayFrame.Core;
using PlayFrame.Systems.Events;
using PlayFrame.Systems.SaveSystem;

namespace PlayFrame.Systems.Audio
{
    /// <summary>
    /// Central audio manager for music and sound effects.
    /// Persists across scenes and integrates with SaveManager for volume settings.
    /// </summary>
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Settings")]
        [SerializeField] private float crossFadeDuration = 1f;
        [SerializeField] private bool playMusicOnStart = true;
        [SerializeField] private AudioClip defaultMusic;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMusicMuted;
        private bool _isSfxMuted;

        /// <summary>
        /// Current music volume (0-1)
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set => SetMusicVolume(value);
        }

        /// <summary>
        /// Current SFX volume (0-1)
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set => SetSfxVolume(value);
        }

        /// <summary>
        /// Is music currently muted
        /// </summary>
        public bool IsMusicMuted
        {
            get => _isMusicMuted;
            set => SetMusicMuted(value);
        }

        /// <summary>
        /// Is SFX currently muted
        /// </summary>
        public bool IsSfxMuted
        {
            get => _isSfxMuted;
            set => SetSfxMuted(value);
        }

        /// <summary>
        /// Is music currently playing
        /// </summary>
        public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;

        /// <summary>
        /// Current music clip
        /// </summary>
        public AudioClip CurrentMusic => musicSource?.clip;

        /// <summary>
        /// Event fired when music volume changes
        /// </summary>
        public event Action<float> OnMusicVolumeChanged;

        /// <summary>
        /// Event fired when SFX volume changes
        /// </summary>
        public event Action<float> OnSfxVolumeChanged;

        protected override void OnSingletonAwake()
        {
            InitializeAudioSources();
            LoadVolumeSettings();

            if (playMusicOnStart && defaultMusic != null)
            {
                PlayMusic(defaultMusic);
            }
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                var musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                var sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void LoadVolumeSettings()
        {
            if (SaveManager.HasInstance)
            {
                var saveData = SaveManager.Instance.CurrentSaveData;
                if (saveData != null)
                {
                    _musicVolume = saveData.musicVolume;
                    _sfxVolume = saveData.sfxVolume;
                    ApplyMusicVolume();
                    ApplySfxVolume();
                }
            }
        }

        #region Music Controls

        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        /// <summary>
        /// Play music with crossfade from current track
        /// </summary>
        public void PlayMusicWithCrossFade(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;

            StartCoroutine(CrossFadeMusic(clip, loop));
        }

        private System.Collections.IEnumerator CrossFadeMusic(AudioClip newClip, bool loop)
        {
            float startVolume = musicSource.volume;

            // Fade out
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / crossFadeDuration;
                yield return null;
            }

            // Switch clip
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in
            while (musicSource.volume < GetEffectiveMusicVolume())
            {
                musicSource.volume += startVolume * Time.deltaTime / crossFadeDuration;
                yield return null;
            }

            musicSource.volume = GetEffectiveMusicVolume();
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Pause music
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }

        /// <summary>
        /// Resume paused music
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }

        /// <summary>
        /// Set music volume (0-1)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            ApplyMusicVolume();
            SaveVolumeSettings();
            OnMusicVolumeChanged?.Invoke(_musicVolume);
            TriggerMusicVolumeEvent();
        }

        /// <summary>
        /// Toggle music mute
        /// </summary>
        public void SetMusicMuted(bool muted)
        {
            _isMusicMuted = muted;
            ApplyMusicVolume();
            TriggerMusicMuteEvent();
        }

        private void ApplyMusicVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = GetEffectiveMusicVolume();
            }
        }

        private float GetEffectiveMusicVolume()
        {
            return _isMusicMuted ? 0f : _musicVolume;
        }

        #endregion

        #region SFX Controls

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null || _isSfxMuted) return;

            sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>
        /// Play a sound effect at a specific position
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null || _isSfxMuted) return;

            AudioSource.PlayClipAtPoint(clip, position, _sfxVolume);
        }

        /// <summary>
        /// Play a sound effect with custom volume
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale)
        {
            if (sfxSource == null || clip == null || _isSfxMuted) return;

            sfxSource.PlayOneShot(clip, _sfxVolume * volumeScale);
        }

        /// <summary>
        /// Set SFX volume (0-1)
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            ApplySfxVolume();
            SaveVolumeSettings();
            OnSfxVolumeChanged?.Invoke(_sfxVolume);
            TriggerSfxVolumeEvent();
        }

        /// <summary>
        /// Toggle SFX mute
        /// </summary>
        public void SetSfxMuted(bool muted)
        {
            _isSfxMuted = muted;
            TriggerSfxMuteEvent();
        }

        private void ApplySfxVolume()
        {
            if (sfxSource != null)
            {
                sfxSource.volume = _sfxVolume;
            }
        }

        #endregion

        #region Volume Persistence

        private void SaveVolumeSettings()
        {
            if (SaveManager.HasInstance)
            {
                var saveData = SaveManager.Instance.CurrentSaveData;
                if (saveData != null)
                {
                    saveData.musicVolume = _musicVolume;
                    saveData.sfxVolume = _sfxVolume;
                    SaveManager.Instance.SaveGame();
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Mute all audio
        /// </summary>
        public void MuteAll()
        {
            SetMusicMuted(true);
            SetSfxMuted(true);
        }

        /// <summary>
        /// Unmute all audio
        /// </summary>
        public void UnmuteAll()
        {
            SetMusicMuted(false);
            SetSfxMuted(false);
        }

        /// <summary>
        /// Reset volumes to default
        /// </summary>
        public void ResetToDefaults()
        {
            SetMusicVolume(1f);
            SetSfxVolume(1f);
            SetMusicMuted(false);
            SetSfxMuted(false);
        }

        #endregion

        #region Event Triggers

        private void TriggerMusicVolumeEvent()
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(GameEvents.MusicVolumeChanged, _musicVolume);
            }
        }

        private void TriggerSfxVolumeEvent()
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(GameEvents.SfxVolumeChanged, _sfxVolume);
            }
        }

        private void TriggerMusicMuteEvent()
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(GameEvents.MusicMuteChanged, _isMusicMuted);
            }
        }

        private void TriggerSfxMuteEvent()
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(GameEvents.SfxMuteChanged, _isSfxMuted);
            }
        }

        #endregion
    }
}
