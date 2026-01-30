using UnityEngine;
using PlayFrame.UI.Prefabs;

namespace PlayFrame.UI
{
    /// <summary>
    /// UI Bootstrap - Initializes UI settings and factory.
    /// Add this component to the same GameObject as GameBootstrap.
    /// </summary>
    public class UIBootstrap : MonoBehaviour
    {
        [Header("UI Settings")]
        [Tooltip("UI Prefab settings from GameSettings/UI")]
        [SerializeField] private UIPrefabSettings uiPrefabSettings;

        private void Awake()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (uiPrefabSettings != null)
            {
                UIPrefabFactory.SetSettings(uiPrefabSettings);
                Debug.Log("[UIBootstrap] UI Prefab Settings initialized");
            }
            else
            {
                Debug.LogWarning("[UIBootstrap] UIPrefabSettings not assigned. " +
                    "Create one via Assets > Create > PlayFrame > UI > UI Prefab Settings " +
                    "and place in GameSettings/UI folder.");
            }
        }
    }
}
