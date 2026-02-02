using UnityEngine;
using UnityEngine.UI;

namespace PlayFrame.Systems.Audio
{
    /// <summary>
    /// Automatically plays click sound on button press.
    /// Attach to any Button to add click sound.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        [SerializeField] private AudioClip customClickSound;
        [SerializeField] private SFXCollection sfxCollection;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(PlayClickSound);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(PlayClickSound);
            }
        }

        private void PlayClickSound()
        {
            if (!AudioManager.HasInstance) return;

            if (customClickSound != null)
            {
                AudioManager.Instance.PlaySFX(customClickSound);
            }
            else if (sfxCollection != null && sfxCollection.buttonClick != null)
            {
                AudioManager.Instance.PlaySFX(sfxCollection.buttonClick);
            }
        }
    }
}
