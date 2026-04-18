using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Systems.Audio;
using PlayFrame.Systems.Canvas;
using PlayFrame.Systems.Game;
using PlayFrame.Systems.Input;
using PlayFrame.Systems.Layout;
using PlayFrame.Systems.Save;
using PlayFrame.Systems.Analytics;
using PlayFrame.Systems.Localization;
using PlayFrame.UI;
using PlayFrame.UI.Panels;

namespace PlayFrame.Systems.Scene.Editor
{
    /// <summary>
    /// Creates pre-configured PlayFrame scenes via the Assets/Create menu.
    /// Access via: Assets → Create → PlayFrame → Scene → [SceneType]
    /// </summary>
    public static class SceneTemplateFactory
    {
        private const string MenuRoot = "Assets/Create/PlayFrame/Scene/";

        // Default colors
        private static readonly Color DarkBg = new Color(0.1f, 0.1f, 0.15f, 1f);
        private static readonly Color PanelBg = new Color(0.12f, 0.12f, 0.16f, 0.95f);
        private static readonly Color ProgressBg = new Color(0.2f, 0.2f, 0.25f, 1f);
        private static readonly Color ProgressFill = new Color(0.2f, 0.6f, 0.9f, 1f);

        // ──────────────────────────────────────────────
        //  BOOTSTRAP SCENE
        // ──────────────────────────────────────────────

        [MenuItem(MenuRoot + "Bootstrap Scene", false, 0)]
        public static void CreateBootstrapScene()
        {
            var sceneName = GetSceneName("Bootstrap");
            if (sceneName == null) return;

            var scene = NewScene(sceneName);

            // Camera
            var cam = CreateCamera();

            // --- Managers ---
            var managers = new GameObject("[Managers]");

            CreateManager<EventManager>(managers.transform, "EventManager");
            CreateManager<SaveManager>(managers.transform, "SaveManager");
            CreateManager<AudioManager>(managers.transform, "AudioManager");
            CreateManager<InputManager>(managers.transform, "InputManager");
            CreateManager<SceneLoaderManager>(managers.transform, "SceneLoaderManager");
            CreateManager<GameRegistry>(managers.transform, "GameRegistry");
            CreateManager<PanelManager>(managers.transform, "PanelManager");
            CreateManager<AnalyticsManager>(managers.transform, "AnalyticsManager");
            CreateManager<LocalizationManager>(managers.transform, "LocalizationManager");

            // --- Bootstrap Controller ---
            var bootstrapGO = new GameObject("GameBootstrap");
            bootstrapGO.AddComponent<GameBootstrap>();

            SaveScene(scene, sceneName);
        }

        // ──────────────────────────────────────────────
        //  MAIN MENU SCENE
        // ──────────────────────────────────────────────

        [MenuItem(MenuRoot + "MainMenu Scene", false, 1)]
        public static void CreateMainMenuScene()
        {
            var sceneName = GetSceneName("MainMenu");
            if (sceneName == null) return;

            var scene = NewScene(sceneName);

            var cam = CreateCamera();
            CreateEventSystem();

            // Canvas
            var canvas = CreateCanvas("MainMenuCanvas");
            var safeArea = CreateSafeArea(canvas.transform);

            // Background
            var bg = CreateUIElement<Image>("Background", safeArea.transform);
            StretchFull(bg.rectTransform);
            bg.color = DarkBg;
            bg.raycastTarget = false;

            // Title
            var title = CreateText("Title", safeArea.transform, "GAME TITLE", 64, TextAlignmentOptions.Center);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.1f, 0.6f);
            titleRect.anchorMax = new Vector2(0.9f, 0.8f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Button container
            var buttonContainer = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            buttonContainer.transform.SetParent(safeArea.transform, false);
            var btnRect = buttonContainer.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.25f, 0.25f);
            btnRect.anchorMax = new Vector2(0.75f, 0.5f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            var vlg = buttonContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 20f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Play button
            CreateMenuButton(buttonContainer.transform, "PlayButton", "PLAY", new Color(0.2f, 0.6f, 0.9f, 1f), 70f);

            // Quit button
            CreateMenuButton(buttonContainer.transform, "QuitButton", "QUIT", new Color(0.4f, 0.4f, 0.45f, 1f), 60f);

            SaveScene(scene, sceneName);
        }

        // ──────────────────────────────────────────────
        //  LOADING SCENE
        // ──────────────────────────────────────────────

        [MenuItem(MenuRoot + "Loading Scene", false, 2)]
        public static void CreateLoadingScene()
        {
            var sceneName = GetSceneName("LoadingScreen");
            if (sceneName == null) return;

            var scene = NewScene(sceneName);

            var cam = CreateCamera();
            CreateEventSystem();

            // Canvas
            var canvas = CreateCanvas("LoadingCanvas");
            var safeArea = CreateSafeArea(canvas.transform);

            // Background
            var bg = CreateUIElement<Image>("Background", safeArea.transform);
            StretchFull(bg.rectTransform);
            bg.color = DarkBg;
            bg.raycastTarget = false;

            // Loading text
            var loadingText = CreateText("LoadingText", safeArea.transform, "Loading...", 36, TextAlignmentOptions.Center);
            var ltRect = loadingText.rectTransform;
            ltRect.anchorMin = new Vector2(0.1f, 0.55f);
            ltRect.anchorMax = new Vector2(0.9f, 0.65f);
            ltRect.offsetMin = Vector2.zero;
            ltRect.offsetMax = Vector2.zero;

            // Progress bar background
            var progressBg = CreateUIElement<Image>("ProgressBarBackground", safeArea.transform);
            progressBg.color = ProgressBg;
            var pbRect = progressBg.rectTransform;
            pbRect.anchorMin = new Vector2(0.15f, 0.42f);
            pbRect.anchorMax = new Vector2(0.85f, 0.46f);
            pbRect.offsetMin = Vector2.zero;
            pbRect.offsetMax = Vector2.zero;

            // Progress bar fill
            var progressFill = CreateUIElement<Image>("ProgressBarFill", progressBg.transform);
            progressFill.color = ProgressFill;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;
            StretchFull(progressFill.rectTransform);

            // Percentage text
            var percentText = CreateText("PercentageText", safeArea.transform, "0%", 28, TextAlignmentOptions.Center);
            var ptRect = percentText.rectTransform;
            ptRect.anchorMin = new Vector2(0.3f, 0.35f);
            ptRect.anchorMax = new Vector2(0.7f, 0.42f);
            ptRect.offsetMin = Vector2.zero;
            ptRect.offsetMax = Vector2.zero;

            // Loading scene controller
            var controllerGO = new GameObject("LoadingSceneController");
            controllerGO.AddComponent<LoadingSceneController>();

            SaveScene(scene, sceneName);
        }

        // ──────────────────────────────────────────────
        //  GAME SCENE
        // ──────────────────────────────────────────────

        [MenuItem(MenuRoot + "Game Scene", false, 3)]
        public static void CreateGameScene()
        {
            var sceneName = GetSceneName("NewGame");
            if (sceneName == null) return;

            var scene = NewScene(sceneName);

            var cam = CreateCamera();
            CreateEventSystem();

            // Canvas
            var canvas = CreateCanvas("GameCanvas");
            var safeArea = CreateSafeArea(canvas.transform);

            // GameLayoutManager on SafeArea
            var layoutManager = safeArea.gameObject.AddComponent<GameLayoutManager>();

            // Background image
            var bgGO = CreateUIElement<Image>("Background", safeArea.transform);
            StretchFull(bgGO.rectTransform);
            bgGO.color = DarkBg;
            bgGO.raycastTarget = false;
            // Move background behind layout zones
            bgGO.transform.SetAsFirstSibling();

            // Top Panel (HUD: score, moves, timer)
            var topPanel = CreateLayoutZone("TopPanel", safeArea.transform);
            var topRect = topPanel.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 0.88f);
            topRect.anchorMax = new Vector2(1f, 1f);

            // Add HorizontalLayoutGroup for HUD items
            var topHlg = topPanel.AddComponent<HorizontalLayoutGroup>();
            topHlg.spacing = 20f;
            topHlg.padding = new RectOffset(20, 20, 10, 10);
            topHlg.childAlignment = TextAnchor.MiddleCenter;
            topHlg.childControlWidth = true;
            topHlg.childControlHeight = true;
            topHlg.childForceExpandWidth = true;
            topHlg.childForceExpandHeight = false;

            // Score label
            CreateText("ScoreLabel", topPanel.transform, "Score: 0", 28, TextAlignmentOptions.Center);

            // Center Panel (Game area / Grid)
            var centerPanel = CreateLayoutZone("CenterPanel", safeArea.transform);
            var centerRect = centerPanel.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0f, 0.1f);
            centerRect.anchorMax = new Vector2(1f, 0.88f);

            // Bottom Panel (Controls / Boosters)
            var bottomPanel = CreateLayoutZone("BottomPanel", safeArea.transform);
            var bottomRect = bottomPanel.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0f, 0f);
            bottomRect.anchorMax = new Vector2(1f, 0.1f);

            var bottomHlg = bottomPanel.AddComponent<HorizontalLayoutGroup>();
            bottomHlg.spacing = 15f;
            bottomHlg.padding = new RectOffset(20, 20, 5, 5);
            bottomHlg.childAlignment = TextAnchor.MiddleCenter;
            bottomHlg.childControlWidth = true;
            bottomHlg.childControlHeight = true;
            bottomHlg.childForceExpandWidth = true;
            bottomHlg.childForceExpandHeight = false;

            // Wire layout manager fields via SerializedObject
            var so = new SerializedObject(layoutManager);
            so.FindProperty("topPanel").objectReferenceValue = topRect;
            so.FindProperty("centerPanel").objectReferenceValue = centerRect;
            so.FindProperty("bottomPanel").objectReferenceValue = bottomRect;
            so.FindProperty("backgroundImage").objectReferenceValue = bgGO;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(scene, sceneName);
        }

        // ──────────────────────────────────────────────
        //  HELPER: Scene Management
        // ──────────────────────────────────────────────

        private static string GetSceneName(string defaultName)
        {
            var name = EditorInputDialog.Show("Create PlayFrame Scene", "Scene name:", defaultName);
            if (string.IsNullOrEmpty(name)) return null;
            return name;
        }

        private static UnityEngine.SceneManagement.Scene NewScene(string name)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return default;
            }

            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string sceneName)
        {
            // Ensure Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            var path = $"Assets/Scenes/{sceneName}.unity";
            EditorSceneManager.SaveScene(scene, path);

            // Add to Build Settings if not already present
            AddToBuildSettings(path);

            Debug.Log($"[PlayFrame] Scene created and saved: {path}");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }

        private static void AddToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (var s in scenes)
            {
                if (s.path == scenePath) return; // Already in build settings
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[PlayFrame] Added to Build Settings: {scenePath}");
        }

        // ──────────────────────────────────────────────
        //  HELPER: GameObject Creation
        // ──────────────────────────────────────────────

        private static Camera CreateCamera()
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = DarkBg;
            cam.orthographic = true;
            return cam;
        }

        private static void CreateEventSystem()
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        private static UnityEngine.Canvas CreateCanvas(string name)
        {
            var canvasGO = new GameObject(name, typeof(UnityEngine.Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Add ResponsiveCanvasSetup
            canvasGO.AddComponent<ResponsiveCanvasSetup>();

            return canvas;
        }

        private static RectTransform CreateSafeArea(Transform parent)
        {
            var safeGO = new GameObject("SafeArea", typeof(RectTransform));
            safeGO.transform.SetParent(parent, false);
            var rect = safeGO.GetComponent<RectTransform>();
            StretchFull(rect);

            safeGO.AddComponent<SafeAreaHandler>();
            return rect;
        }

        private static GameObject CreateLayoutZone(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            StretchFull(rect);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        private static T CreateManager<T>(Transform parent, string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        private static T CreateUIElement<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var component = go.AddComponent<T>();
            return component;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void CreateMenuButton(Transform parent, string name, string label, Color color, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.color = color;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            StretchFull(textGO.GetComponent<RectTransform>());
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// Simple editor input dialog for scene name input.
    /// </summary>
    public class EditorInputDialog : EditorWindow
    {
        private string _input;
        private string _message;
        private bool _confirmed;
        private bool _closed;
        private static string _result;

        public static string Show(string title, string message, string defaultValue)
        {
            _result = null;
            var window = CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent(title);
            window._message = message;
            window._input = defaultValue;
            window._confirmed = false;
            window._closed = false;
            window.minSize = new Vector2(350, 120);
            window.maxSize = new Vector2(350, 120);
            window.ShowModalUtility();
            return _result;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(_message);
            EditorGUILayout.Space(5);

            GUI.SetNextControlName("InputField");
            _input = EditorGUILayout.TextField(_input);
            EditorGUI.FocusTextInControl("InputField");

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                _result = null;
                Close();
            }

            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                _result = _input;
                Close();
            }

            EditorGUILayout.EndHorizontal();

            // Enter key to confirm
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                _result = _input;
                Close();
            }
        }
    }
}
