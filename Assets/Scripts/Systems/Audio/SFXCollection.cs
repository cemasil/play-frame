using UnityEngine;

namespace MiniGameFramework.Systems.Audio
{
    /// <summary>
    /// ScriptableObject for organizing sound effects by category
    /// </summary>
    [CreateAssetMenu(fileName = "SFXCollection", menuName = "MiniGameFramework/Audio/SFX Collection")]
    public class SFXCollection : ScriptableObject
    {
        [Header("UI Sounds")]
        public AudioClip buttonClick;
        public AudioClip buttonHover;
        public AudioClip panelOpen;
        public AudioClip panelClose;

        [Header("Game Sounds")]
        public AudioClip match;
        public AudioClip mismatch;
        public AudioClip collect;
        public AudioClip drop;
        public AudioClip swap;

        [Header("Result Sounds")]
        public AudioClip win;
        public AudioClip lose;
        public AudioClip levelUp;
        public AudioClip highScore;

        [Header("Feedback Sounds")]
        public AudioClip success;
        public AudioClip error;
        public AudioClip notification;
        public AudioClip countdown;

        /// <summary>
        /// Play a clip through AudioManager
        /// </summary>
        public void Play(AudioClip clip)
        {
            if (clip != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }

        // Convenience methods
        public void PlayButtonClick() => Play(buttonClick);
        public void PlayMatch() => Play(match);
        public void PlayMismatch() => Play(mismatch);
        public void PlayWin() => Play(win);
        public void PlayLose() => Play(lose);
    }
}
