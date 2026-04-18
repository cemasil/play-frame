using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using PlayFrame.Systems.Build;

namespace PlayFrame.Systems.Build.Editor
{
    /// <summary>
    /// Build pipeline with automatic version management and multi-platform support.
    /// Access via: Tools → PlayFrame → Build → [Action]
    /// </summary>
    public static class BuildPipeline
    {
        private const string MenuRoot = "Tools/PlayFrame/Build/";
        private const string ConfigPath = "Assets/GameSettings/BuildConfig.asset";
        private const string DevConfigPath = "Assets/GameSettings/BuildConfig_Dev.asset";
        private const string ProdConfigPath = "Assets/GameSettings/BuildConfig_Prod.asset";

        // ──────────────────────────────────────────────
        //  MENU ITEMS
        // ──────────────────────────────────────────────

        [MenuItem(MenuRoot + "Increment Patch Version", false, 10)]
        public static void IncrementPatch()
        {
            var config = LoadOrCreateConfig();
            var parts = config.version.Split('.');
            if (parts.Length == 3 && int.TryParse(parts[2], out int patch))
            {
                parts[2] = (patch + 1).ToString();
                config.version = string.Join(".", parts);
                SaveConfig(config);
                Debug.Log($"[Build] Version → {config.version}");
            }
        }

        [MenuItem(MenuRoot + "Increment Minor Version", false, 11)]
        public static void IncrementMinor()
        {
            var config = LoadOrCreateConfig();
            var parts = config.version.Split('.');
            if (parts.Length == 3 && int.TryParse(parts[1], out int minor))
            {
                parts[1] = (minor + 1).ToString();
                parts[2] = "0";
                config.version = string.Join(".", parts);
                SaveConfig(config);
                Debug.Log($"[Build] Version → {config.version}");
            }
        }

        [MenuItem(MenuRoot + "Increment Major Version", false, 12)]
        public static void IncrementMajor()
        {
            var config = LoadOrCreateConfig();
            var parts = config.version.Split('.');
            if (parts.Length == 3 && int.TryParse(parts[0], out int major))
            {
                parts[0] = (major + 1).ToString();
                parts[1] = "0";
                parts[2] = "0";
                config.version = string.Join(".", parts);
                SaveConfig(config);
                Debug.Log($"[Build] Version → {config.version}");
            }
        }

        [MenuItem(MenuRoot + "Create Build Configs", false, 30)]
        public static void CreateBuildConfigs()
        {
            EnsureFolder("Assets/GameSettings");

            // Dev config
            if (!File.Exists(DevConfigPath))
            {
                var dev = ScriptableObject.CreateInstance<BuildConfig>();
                dev.environment = BuildEnvironment.Development;
                dev.developmentBuild = true;
                dev.autoConnectProfiler = true;
                dev.allowDebugging = true;
                dev.devBundleIdSuffix = ".dev";
                AssetDatabase.CreateAsset(dev, DevConfigPath);
                Debug.Log($"[Build] Created dev config: {DevConfigPath}");
            }

            // Prod config
            if (!File.Exists(ProdConfigPath))
            {
                var prod = ScriptableObject.CreateInstance<BuildConfig>();
                prod.environment = BuildEnvironment.Production;
                prod.developmentBuild = false;
                prod.autoConnectProfiler = false;
                prod.allowDebugging = false;
                prod.deepProfiling = false;
                prod.prodBundleIdSuffix = "";
                AssetDatabase.CreateAsset(prod, ProdConfigPath);
                Debug.Log($"[Build] Created prod config: {ProdConfigPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Build] Build configs ready in Assets/GameSettings/");
        }

        [MenuItem(MenuRoot + "Build Android Dev", false, 50)]
        public static void BuildAndroidDev()
        {
            Build(BuildTarget.Android, BuildEnvironment.Development);
        }

        [MenuItem(MenuRoot + "Build Android Prod", false, 51)]
        public static void BuildAndroidProd()
        {
            Build(BuildTarget.Android, BuildEnvironment.Production);
        }

        [MenuItem(MenuRoot + "Build iOS Dev", false, 52)]
        public static void BuildIOSDev()
        {
            Build(BuildTarget.iOS, BuildEnvironment.Development);
        }

        [MenuItem(MenuRoot + "Build iOS Prod", false, 53)]
        public static void BuildIOSProd()
        {
            Build(BuildTarget.iOS, BuildEnvironment.Production);
        }

        // ──────────────────────────────────────────────
        //  BUILD LOGIC
        // ──────────────────────────────────────────────

        private static void Build(BuildTarget target, BuildEnvironment env)
        {
            var configPath = env == BuildEnvironment.Development ? DevConfigPath : ProdConfigPath;
            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(configPath);

            if (config == null)
            {
                Debug.LogError($"[Build] Config not found at {configPath}. Run Tools → PlayFrame → Build → Create Build Configs first.");
                return;
            }

            // Auto-increment build number
            config.buildNumber++;
            SaveConfig(config);

            // Apply version to PlayerSettings
            PlayerSettings.bundleVersion = config.version;

            string baseBundleId = PlayerSettings.applicationIdentifier;
            if (baseBundleId.EndsWith(".dev"))
                baseBundleId = baseBundleId.Replace(".dev", "");

            string finalBundleId = baseBundleId + config.BundleIdSuffix;

            if (target == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode = config.buildNumber;
                PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, finalBundleId);
            }
            else if (target == BuildTarget.iOS)
            {
                PlayerSettings.iOS.buildNumber = config.buildNumber.ToString();
                PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, finalBundleId);
            }

            // Build options
            var options = BuildOptions.None;
            if (config.developmentBuild)
                options |= BuildOptions.Development;
            if (config.autoConnectProfiler)
                options |= BuildOptions.ConnectWithProfiler;
            if (config.deepProfiling)
                options |= BuildOptions.EnableDeepProfilingSupport;
            if (config.allowDebugging)
                options |= BuildOptions.AllowDebugging;

            // Build path
            string envLabel = env == BuildEnvironment.Development ? "Dev" : "Prod";
            string platformLabel = target == BuildTarget.Android ? "Android" : "iOS";
            string extension = target == BuildTarget.Android ? (EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk") : "";
            string buildFolder = $"Builds/{platformLabel}_{envLabel}";
            string buildPath = $"{buildFolder}/{PlayerSettings.productName}_{config.version}_b{config.buildNumber}{extension}";

            Directory.CreateDirectory(buildFolder);

            // Collect enabled scenes
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = buildPath,
                target = target,
                options = options
            };

            Debug.Log($"[Build] Starting {platformLabel} {envLabel} build: v{config.FullVersion}");
            var report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] ✓ {platformLabel} {envLabel} build succeeded: {buildPath} ({report.summary.totalSize / 1048576f:F1} MB)");
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"[Build] ✗ Build failed: {report.summary.result}");
            }
        }

        // ──────────────────────────────────────────────
        //  HELPERS
        // ──────────────────────────────────────────────

        private static BuildConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(ConfigPath);
            if (config == null)
            {
                EnsureFolder("Assets/GameSettings");
                config = ScriptableObject.CreateInstance<BuildConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
            }
            return config;
        }

        private static void SaveConfig(BuildConfig config)
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }

    /// <summary>
    /// Automatically applies BuildConfig version to PlayerSettings before each build.
    /// </summary>
    public class BuildVersionPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>("Assets/GameSettings/BuildConfig.asset");
            if (config == null) return;

            PlayerSettings.bundleVersion = config.version;

            if (report.summary.platform == BuildTarget.Android)
                PlayerSettings.Android.bundleVersionCode = config.buildNumber;
            else if (report.summary.platform == BuildTarget.iOS)
                PlayerSettings.iOS.buildNumber = config.buildNumber.ToString();

            Debug.Log($"[Build] Pre-build: v{config.FullVersion}");
        }
    }
}
