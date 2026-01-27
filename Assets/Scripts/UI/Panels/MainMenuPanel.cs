using UnityEngine;
using UnityEngine.UI;
using MiniGameFramework.Systems.SceneManagement;
using MiniGameFramework.Systems.UI;

namespace MiniGameFramework.UI.Panels
{
    public class MainMenuPanel : UIPanel
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        protected override void OnInitialize()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.GAME_SELECTION);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        protected override void OnCleanup()
        {
            if (playButton != null)
                playButton.onClick.RemoveAllListeners();

            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    }
}
