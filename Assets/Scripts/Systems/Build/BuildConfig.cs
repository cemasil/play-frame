using UnityEngine;

namespace PlayFrame.Systems.Build
{
    /// <summary>
    /// ScriptableObject holding build version information.
    /// Auto-incremented by BuildPipeline editor tools.
    /// Create via: Assets → Create → PlayFrame → Build → Build Config
    /// </summary>
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "PlayFrame/Build/Build Config")]
    public class BuildConfig : ScriptableObject
    {
        [Header("Version")]
        [Tooltip("Semantic version: MAJOR.MINOR.PATCH")]
        public string version = "1.0.0";

        [Tooltip("Auto-incremented on each build")]
        public int buildNumber = 1;

        [Header("Environment")]
        public BuildEnvironment environment = BuildEnvironment.Development;

        [Header("Platform Overrides")]
        [Tooltip("Override bundle ID suffix per environment (e.g., '.dev' for dev builds)")]
        public string devBundleIdSuffix = ".dev";
        public string prodBundleIdSuffix = "";

        [Header("Build Options")]
        public bool developmentBuild = true;
        public bool autoConnectProfiler = false;
        public bool deepProfiling = false;
        public bool allowDebugging = true;

        /// <summary>
        /// Full version string: "1.0.0 (42)"
        /// </summary>
        public string FullVersion => $"{version} ({buildNumber})";

        /// <summary>
        /// Short version string for display: "1.0.0"
        /// </summary>
        public string ShortVersion => version;

        /// <summary>
        /// Whether this is a development build
        /// </summary>
        public bool IsDevelopment => environment == BuildEnvironment.Development;

        /// <summary>
        /// Get bundle ID suffix for current environment
        /// </summary>
        public string BundleIdSuffix => environment == BuildEnvironment.Development ? devBundleIdSuffix : prodBundleIdSuffix;
    }

    public enum BuildEnvironment
    {
        Development,
        Production
    }
}
