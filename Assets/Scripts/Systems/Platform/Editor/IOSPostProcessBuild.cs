using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace PlayFrame.Systems.Platform.Editor
{
    /// <summary>
    /// Post-process build hook for iOS Xcode project configuration.
    /// Automatically applies:
    /// - Info.plist privacy descriptions (ATT, photo library, etc.)
    /// - Team ID and automatic signing
    /// - Required frameworks
    /// </summary>
    public static class IOSPostProcessBuild
    {
        [PostProcessBuild(100)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if UNITY_IOS
            if (target != BuildTarget.iOS) return;

            ApplyInfoPlist(pathToBuiltProject);
            ApplyXcodeProject(pathToBuiltProject);

            Debug.Log("[PlayFrame] iOS post-process build completed.");
#endif
        }

#if UNITY_IOS
        private static void ApplyInfoPlist(string buildPath)
        {
            string plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new UnityEditor.iOS.Xcode.PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            var root = plist.root;

            // App Tracking Transparency
            root.SetString("NSUserTrackingUsageDescription",
                "This identifier will be used to deliver personalized ads to you.");

            // Add other required privacy descriptions as needed:
            // root.SetString("NSPhotoLibraryUsageDescription", "Used to save screenshots.");
            // root.SetString("NSCameraUsageDescription", "Used for AR features.");

            File.WriteAllText(plistPath, plist.WriteToString());
            Debug.Log("[PlayFrame] Info.plist updated with privacy descriptions.");
        }

        private static void ApplyXcodeProject(string buildPath)
        {
            string projectPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(buildPath);
            var project = new UnityEditor.iOS.Xcode.PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));

            string targetGuid = project.GetUnityMainTargetGuid();
            string frameworkGuid = project.GetUnityFrameworkTargetGuid();

            // Automatic signing
            project.SetBuildProperty(targetGuid, "CODE_SIGN_STYLE", "Automatic");

            // Team ID — set from PlayerSettings or override here
            // project.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", "YOUR_TEAM_ID");

            // Enable Bitcode (off for most Unity projects)
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(frameworkGuid, "ENABLE_BITCODE", "NO");

            // Required frameworks
            project.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", true);
            project.AddFrameworkToProject(targetGuid, "AdSupport.framework", true);

            File.WriteAllText(projectPath, project.WriteToString());
            Debug.Log("[PlayFrame] Xcode project updated (signing, frameworks).");
        }
#endif
    }
}
