using UnityEngine;

namespace PlayFrame.UI
{
    /// <summary>
    /// UI Bootstrap - Initializes UI settings and factory.
    /// Add this component to the same GameObject as GameBootstrap.
    /// </summary>
    public class UIBootstrap : MonoBehaviour
    {
        [Header("UI Settings")]
        [Tooltip("Optional: Assign settings directly. If null, uses default settings")]
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
                // Create default settings if not assigned
                var defaultSettings = UIPrefabSettings.CreateDefault();
                UIPrefabFactory.SetSettings(defaultSettings);
            }
        }
    }
}
