using UnityEngine;

namespace MiniGameFramework.Systems.Audio
{
    /// <summary>
    /// ScriptableObject for organizing music tracks
    /// </summary>
    [CreateAssetMenu(fileName = "MusicCollection", menuName = "MiniGameFramework/Audio/Music Collection")]
    public class MusicCollection : ScriptableObject
    {
        [Header("Menu Music")]
        public AudioClip mainMenu;
        public AudioClip gameSelection;

        [Header("Game Music")]
        public AudioClip[] gameTracks;

        [Header("Result Music")]
        public AudioClip victory;
        public AudioClip defeat;

        /// <summary>
        /// Play a specific track through AudioManager
        /// </summary>
        public void Play(AudioClip clip, bool loop = true)
        {
            if (clip != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayMusic(clip, loop);
            }
        }

        /// <summary>
        /// Play with crossfade
        /// </summary>
        public void PlayWithCrossFade(AudioClip clip, bool loop = true)
        {
            if (clip != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayMusicWithCrossFade(clip, loop);
            }
        }

        /// <summary>
        /// Play a random game track
        /// </summary>
        public void PlayRandomGameTrack()
        {
            if (gameTracks != null && gameTracks.Length > 0)
            {
                var randomTrack = gameTracks[Random.Range(0, gameTracks.Length)];
                Play(randomTrack);
            }
        }

        // Convenience methods
        public void PlayMainMenu() => Play(mainMenu);
        public void PlayVictory() => Play(victory, false);
        public void PlayDefeat() => Play(defeat, false);
    }
}
