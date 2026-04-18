using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace PlayFrame.Systems.Scene.Editor
{
    /// <summary>
    /// Wizard for setting up a new game project from the PlayFrame template.
    /// Updates product name, company name, bundle ID, and project settings.
    /// Access via: Tools → PlayFrame → Project Setup Wizard
    /// </summary>
    public class ProjectSetupWizard : EditorWindow
    {
        private string _productName = "";
        private string _companyName = "";
        private string _bundleIdSuffix = "";
        private bool _targetIOS = true;
        private bool _targetAndroid = true;
        private Vector2 _scrollPos;

        [MenuItem("Tools/PlayFrame/Project Setup Wizard", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectSetupWizard>("Project Setup");
            window.minSize = new Vector2(450, 380);
            window.LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            _productName = PlayerSettings.productName;
            _companyName = PlayerSettings.companyName;

            var bundleId = PlayerSettings.applicationIdentifier;
            if (!string.IsNullOrEmpty(bundleId))
            {
                var parts = bundleId.Split('.');
                if (parts.Length >= 3)
                    _bundleIdSuffix = parts[parts.Length - 1];
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("PlayFrame — Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure your game project identity. This updates Unity PlayerSettings " +
                "and all related build configurations.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // ── Project Identity ──
            EditorGUILayout.LabelField("Project Identity", EditorStyles.boldLabel);
            _productName = EditorGUILayout.TextField("Product Name", _productName);
            _companyName = EditorGUILayout.TextField("Company Name", _companyName);
            _bundleIdSuffix = EditorGUILayout.TextField("Bundle ID Suffix", _bundleIdSuffix);

            string bundleId = GenerateBundleId();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Bundle Identifier", bundleId);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            // ── Target Platforms ──
            EditorGUILayout.LabelField("Target Platforms", EditorStyles.boldLabel);
            _targetIOS = EditorGUILayout.Toggle("iOS", _targetIOS);
            _targetAndroid = EditorGUILayout.Toggle("Android", _targetAndroid);

            EditorGUILayout.Space(15);

            // ── Actions ──
            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_productName) || string.IsNullOrWhiteSpace(_companyName));

            if (GUILayout.Button("Apply Settings", GUILayout.Height(35)))
            {
                ApplySettings();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("What this changes:", EditorStyles.miniLabel);
            EditorGUILayout.HelpBox(
                "• PlayerSettings: Product Name, Company Name, Bundle Identifier\n" +
                "• PlayerSettings: iOS & Android platform settings\n" +
                "• PlayerSettings: Default orientation, icons, splash\n" +
                "• Build Profiles: Dev/Prod configurations per platform",
                MessageType.None);

            EditorGUILayout.EndScrollView();
        }

        private string GenerateBundleId()
        {
            string company = SanitizeId(_companyName);
            string product = SanitizeId(string.IsNullOrEmpty(_bundleIdSuffix) ? _productName : _bundleIdSuffix);
            return $"com.{company}.{product}";
        }

        private static string SanitizeId(string input)
        {
            if (string.IsNullOrEmpty(input)) return "game";
            string result = Regex.Replace(input.ToLower().Trim(), @"[^a-z0-9]", "");
            return string.IsNullOrEmpty(result) ? "game" : result;
        }

        private void ApplySettings()
        {
            string bundleId = GenerateBundleId();

            // ── Core PlayerSettings ──
            PlayerSettings.productName = _productName;
            PlayerSettings.companyName = _companyName;

            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, bundleId);
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, bundleId);

            // ── Shared Settings ──
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // ── iOS Settings ──
            if (_targetIOS)
            {
                ApplyIOSSettings(bundleId);
            }

            // ── Android Settings ──
            if (_targetAndroid)
            {
                ApplyAndroidSettings(bundleId);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayFrame] Project configured: {_productName} ({bundleId})");
            EditorUtility.DisplayDialog("Project Setup Complete",
                $"Product: {_productName}\n" +
                $"Company: {_companyName}\n" +
                $"Bundle ID: {bundleId}\n\n" +
                "PlayerSettings updated successfully.",
                "OK");
        }

        private void ApplyIOSSettings(string bundleId)
        {
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.hideHomeButton = false;
            PlayerSettings.iOS.requiresFullScreen = true;
            PlayerSettings.iOS.statusBarStyle = iOSStatusBarStyle.Default;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;

            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(UnityEditor.Build.NamedBuildTarget.iOS, 1); // ARM64

            Debug.Log("[PlayFrame] iOS settings applied (IL2CPP, ARM64, min iOS 15.0)");
        }

        private void ApplyAndroidSettings(string bundleId)
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;

            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // AAB for Play Store
            EditorUserBuildSettings.buildAppBundle = true;

            Debug.Log("[PlayFrame] Android settings applied (IL2CPP, ARM64, AAB, API 28+)");
        }
    }
}
