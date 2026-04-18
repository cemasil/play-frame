using UnityEngine;
using System.Collections;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Platform
{
    /// <summary>
    /// Handles iOS App Tracking Transparency (ATT) permission prompt.
    /// Add to a persistent GameObject (e.g., Bootstrap scene).
    /// Automatically requests tracking permission on iOS 14.5+.
    /// No-op on non-iOS platforms.
    /// </summary>
    public class IOSTrackingPermission : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Delay before showing the ATT dialog (seconds). Apple recommends waiting until the user has context.")]
        [SerializeField] private float promptDelay = 1.0f;

        private static readonly ILogger _logger = LoggerFactory.CreateCore("IOSTracking");
        private const string PERMISSION_REQUESTED_KEY = "iOS_ATTracking_Requested";

        private void Start()
        {
#if UNITY_IOS && !UNITY_EDITOR
            StartCoroutine(RequestTrackingPermission());
#endif
        }

        private IEnumerator RequestTrackingPermission()
        {
#if UNITY_IOS
            if (PlayerPrefs.GetInt(PERMISSION_REQUESTED_KEY, 0) == 1)
            {
                _logger.Log("ATT permission already requested. Skipping.");
                yield break;
            }

            yield return new WaitForSeconds(promptDelay);

            var status = Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            if (status == Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                _logger.Log("Requesting ATT permission...");
                Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
            else
            {
                _logger.Log($"ATT status already determined: {status}");
            }

            PlayerPrefs.SetInt(PERMISSION_REQUESTED_KEY, 1);
            PlayerPrefs.Save();
#else
            yield break;
#endif
        }
    }
}
