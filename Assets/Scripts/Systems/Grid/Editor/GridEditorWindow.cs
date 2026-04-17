using UnityEngine;
using UnityEditor;
using PlayFrame.Systems.Grid;

namespace PlayFrame.Systems.Grid.Editor
{
    /// <summary>
    /// Editor window for designing and configuring grids visually.
    /// Open via: Tools -> PlayFrame -> Grid Designer
    /// </summary>
    public class GridEditorWindow : EditorWindow
    {
        private GridConfig _config;
        private Vector2 _scrollPosition;
        private float _previewCellSize = 30f;
        private float _previewSpacing = 2f;
        private int _selectedTab = 0;
        private readonly string[] _tabs = { "Grid", "Interaction", "Spawn", "Visuals", "Audio", "Preview" };

        [MenuItem("Tools/PlayFrame/Grid Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<GridEditorWindow>("Grid Designer");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("PlayFrame Grid Designer", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _config = (GridConfig)EditorGUILayout.ObjectField("Grid Config", _config, typeof(GridConfig), false);

            if (_config == null)
            {
                EditorGUILayout.HelpBox("Assign a GridConfig asset to edit, or create a new one.", MessageType.Info);

                if (GUILayout.Button("Create New Grid Config"))
                {
                    CreateNewConfig();
                }
                return;
            }

            EditorGUILayout.Space(5);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);
            EditorGUILayout.Space(5);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0: DrawGridTab(); break;
                case 1: DrawInteractionTab(); break;
                case 2: DrawSpawnTab(); break;
                case 3: DrawVisualsTab(); break;
                case 4: DrawAudioTab(); break;
                case 5: DrawPreviewTab(); break;
            }

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_config);
            }
        }

        #region Grid Tab

        private void DrawGridTab()
        {
            EditorGUILayout.LabelField("Grid Dimensions", EditorStyles.boldLabel);

            var serializedConfig = new SerializedObject(_config);
            serializedConfig.Update();

            EditorGUILayout.PropertyField(serializedConfig.FindProperty("shape"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("columns"));

            if (_config.shape != GridShape.Square)
            {
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("rows"));
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Cell Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("cellSize"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("cellSpacing"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("defaultCellInteraction"));

            // Show grid size info
            Vector2 gridSize = _config.GetGridPixelSize();
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                $"Grid Size: {gridSize.x:F0} x {gridSize.y:F0} px\n" +
                $"Total Cells: {_config.columns * _config.rows}\n" +
                $"Active Cells: {CountActiveCells()}",
                MessageType.Info
            );

            // Custom cell mask editor for Custom shape
            if (_config.shape == GridShape.Custom)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Custom Cell Mask", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Click cells to toggle active/inactive", MessageType.Info);
                DrawCellMaskEditor();
            }

            serializedConfig.ApplyModifiedProperties();
        }

        private void DrawCellMaskEditor()
        {
            if (_config.customCellMask == null || _config.customCellMask.Length != _config.columns * _config.rows)
            {
                if (GUILayout.Button("Initialize Cell Mask"))
                {
                    _config.customCellMask = new bool[_config.columns * _config.rows];
                    for (int i = 0; i < _config.customCellMask.Length; i++)
                        _config.customCellMask[i] = true;
                    EditorUtility.SetDirty(_config);
                }
                return;
            }

            float cellSize = 25f;
            float totalWidth = _config.columns * (cellSize + 2);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(totalWidth, _config.rows * (cellSize + 2));

            for (int row = 0; row < _config.rows; row++)
            {
                for (int col = 0; col < _config.columns; col++)
                {
                    int index = row * _config.columns + col;
                    var cellRect = new Rect(
                        rect.x + col * (cellSize + 2),
                        rect.y + row * (cellSize + 2),
                        cellSize,
                        cellSize
                    );

                    bool isActive = _config.customCellMask[index];
                    EditorGUI.DrawRect(cellRect, isActive ? Color.green : new Color(0.3f, 0.3f, 0.3f));

                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        _config.customCellMask[index] = !_config.customCellMask[index];
                        EditorUtility.SetDirty(_config);
                        Event.current.Use();
                        Repaint();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill All"))
            {
                for (int i = 0; i < _config.customCellMask.Length; i++)
                    _config.customCellMask[i] = true;
                EditorUtility.SetDirty(_config);
            }
            if (GUILayout.Button("Clear All"))
            {
                for (int i = 0; i < _config.customCellMask.Length; i++)
                    _config.customCellMask[i] = false;
                EditorUtility.SetDirty(_config);
            }
            if (GUILayout.Button("Invert"))
            {
                for (int i = 0; i < _config.customCellMask.Length; i++)
                    _config.customCellMask[i] = !_config.customCellMask[i];
                EditorUtility.SetDirty(_config);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Interaction Tab

        private void DrawInteractionTab()
        {
            EditorGUILayout.LabelField("Interaction Modes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select which interaction modes are available for this grid.", MessageType.Info);

            var serializedConfig = new SerializedObject(_config);
            serializedConfig.Update();

            EditorGUILayout.PropertyField(serializedConfig.FindProperty("interactionMode"));

            EditorGUILayout.Space(10);

            // Show mode-specific settings
            if (_config.HasInteraction(GridInteractionMode.MultiSelect))
            {
                EditorGUILayout.LabelField("Multi-Select Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("multiSelectCount"));
            }

            if (_config.HasInteraction(GridInteractionMode.Chain) || _config.HasInteraction(GridInteractionMode.Draw))
            {
                EditorGUILayout.LabelField("Chain/Draw Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("minChainLength"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("allowDiagonals"));
            }

            // Mode descriptions
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Mode Descriptions", EditorStyles.boldLabel);

            if (_config.HasInteraction(GridInteractionMode.Tap))
                EditorGUILayout.HelpBox("TAP: Tap a piece to trigger an action.", MessageType.None);
            if (_config.HasInteraction(GridInteractionMode.Swap))
                EditorGUILayout.HelpBox("SWAP: Swipe between adjacent pieces to swap them (Match-3 style).", MessageType.None);
            if (_config.HasInteraction(GridInteractionMode.DragAndDrop))
                EditorGUILayout.HelpBox("DRAG & DROP: Drag a piece to another cell position.", MessageType.None);
            if (_config.HasInteraction(GridInteractionMode.MultiSelect))
                EditorGUILayout.HelpBox($"MULTI-SELECT: Tap {_config.multiSelectCount} pieces, then an action triggers.", MessageType.None);
            if (_config.HasInteraction(GridInteractionMode.Draw))
                EditorGUILayout.HelpBox("DRAW: Trace a path through pieces without lifting finger.", MessageType.None);
            if (_config.HasInteraction(GridInteractionMode.Chain))
                EditorGUILayout.HelpBox($"CHAIN: Connect adjacent same-type pieces. Minimum chain: {_config.minChainLength}.", MessageType.None);

            serializedConfig.ApplyModifiedProperties();
        }

        #endregion

        #region Spawn Tab

        private void DrawSpawnTab()
        {
            EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);

            var serializedConfig = new SerializedObject(_config);
            serializedConfig.Update();

            EditorGUILayout.PropertyField(serializedConfig.FindProperty("spawnMode"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("spawnAnimationDuration"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("spawnStaggerDelay"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("useSpawnEasing"));

            EditorGUILayout.Space(10);

            // Spawn mode descriptions
            switch (_config.spawnMode)
            {
                case GridSpawnMode.FallFromTop:
                    EditorGUILayout.HelpBox("Pieces fall from above the grid into empty cells.", MessageType.Info);
                    break;
                case GridSpawnMode.RiseFromBottom:
                    EditorGUILayout.HelpBox("Pieces rise from below the grid into empty cells.", MessageType.Info);
                    break;
                case GridSpawnMode.SlideFromLeft:
                    EditorGUILayout.HelpBox("Pieces slide from the left side into empty cells.", MessageType.Info);
                    break;
                case GridSpawnMode.SlideFromRight:
                    EditorGUILayout.HelpBox("Pieces slide from the right side into empty cells.", MessageType.Info);
                    break;
                case GridSpawnMode.SpawnInPlace:
                    EditorGUILayout.HelpBox("Pieces appear at destroyed positions with a fade/scale animation.", MessageType.Info);
                    break;
                case GridSpawnMode.InitialOnly:
                    EditorGUILayout.HelpBox("Pieces are placed only during initialization. No respawn.", MessageType.Info);
                    break;
            }

            serializedConfig.ApplyModifiedProperties();
        }

        #endregion

        #region Visuals Tab

        private void DrawVisualsTab()
        {
            EditorGUILayout.LabelField("Visual Configuration", EditorStyles.boldLabel);

            var serializedConfig = new SerializedObject(_config);
            serializedConfig.Update();

            EditorGUILayout.PropertyField(serializedConfig.FindProperty("visualConfig"));

            if (_config.visualConfig != null)
            {
                EditorGUILayout.Space(5);

                var visualSO = new SerializedObject(_config.visualConfig);
                visualSO.Update();

                EditorGUILayout.LabelField("Grid Background", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridBackground"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridBackgroundColor"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingLeft"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingRight"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingTop"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingBottom"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Grid Border", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridBorderSprite"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("gridBorderColor"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("borderWidth"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("dynamicBorder"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Cell Visuals", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(visualSO.FindProperty("cellBackground"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("cellBackgroundColor"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(visualSO.FindProperty("selectionOverlay"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("selectionColor"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("selectionScale"));

                if (_config.HasInteraction(GridInteractionMode.DragAndDrop))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Drag & Drop", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("dragScale"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("dragAlpha"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("showDragGhost"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("dragGhostAlpha"));
                }

                if (_config.HasInteraction(GridInteractionMode.Chain) || _config.HasInteraction(GridInteractionMode.Draw))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Chain/Draw", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("chainLineMaterial"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("chainLineColor"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("chainLineWidth"));
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Match/Destroy", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(visualSO.FindProperty("destroyParticlePrefab"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("matchScaleAnimation"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("matchHighlightColor"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Spawn", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(visualSO.FindProperty("spawnStartScale"));
                EditorGUILayout.PropertyField(visualSO.FindProperty("spawnStartAlpha"));

                visualSO.ApplyModifiedProperties();
            }
            else
            {
                if (GUILayout.Button("Create Visual Config"))
                {
                    CreateSubConfig<GridVisualConfig>("GridVisualConfig", "visualConfig");
                }
            }

            serializedConfig.ApplyModifiedProperties();
        }

        #endregion

        #region Audio Tab

        private void DrawAudioTab()
        {
            EditorGUILayout.LabelField("Audio Configuration", EditorStyles.boldLabel);

            var serializedConfig = new SerializedObject(_config);
            serializedConfig.Update();

            EditorGUILayout.PropertyField(serializedConfig.FindProperty("audioConfig"));

            if (_config.audioConfig != null)
            {
                var audioSO = new SerializedObject(_config.audioConfig);
                audioSO.Update();

                // Only show relevant audio sections based on interaction mode
                if (_config.HasInteraction(GridInteractionMode.Tap) || _config.HasInteraction(GridInteractionMode.MultiSelect))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Tap/Select Audio", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(audioSO.FindProperty("tapSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("deselectSound"));
                }

                if (_config.HasInteraction(GridInteractionMode.Swap))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Swap Audio", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(audioSO.FindProperty("swapStartSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("swapSuccessSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("swapFailSound"));
                }

                if (_config.HasInteraction(GridInteractionMode.DragAndDrop))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Drag & Drop Audio", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(audioSO.FindProperty("dragStartSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("dragOverCellSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("dropSuccessSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("dropFailSound"));
                }

                if (_config.HasInteraction(GridInteractionMode.Chain) || _config.HasInteraction(GridInteractionMode.Draw))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Chain/Draw Audio", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(audioSO.FindProperty("chainAddSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("chainRemoveSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("chainConfirmSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("chainFailSound"));
                }

                if (_config.HasInteraction(GridInteractionMode.MultiSelect))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Multi-Select Audio", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(audioSO.FindProperty("multiSelectSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("multiSelectCompleteSound"));
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Match/Destroy Audio", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(audioSO.FindProperty("matchSound"));
                EditorGUILayout.PropertyField(audioSO.FindProperty("destroySound"));
                EditorGUILayout.PropertyField(audioSO.FindProperty("comboSound"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Spawn Audio", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(audioSO.FindProperty("spawnSound"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Special Audio", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(audioSO.FindProperty("hintSound"));
                EditorGUILayout.PropertyField(audioSO.FindProperty("noMovesSound"));
                EditorGUILayout.PropertyField(audioSO.FindProperty("shuffleSound"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(audioSO.FindProperty("pitchVariation"));
                EditorGUILayout.PropertyField(audioSO.FindProperty("minSoundInterval"));

                audioSO.ApplyModifiedProperties();
            }
            else
            {
                if (GUILayout.Button("Create Audio Config"))
                {
                    CreateSubConfig<GridAudioConfig>("GridAudioConfig", "audioConfig");
                }
            }

            serializedConfig.ApplyModifiedProperties();
        }

        #endregion

        #region Preview Tab

        private void DrawPreviewTab()
        {
            EditorGUILayout.LabelField("Grid Preview", EditorStyles.boldLabel);

            _previewCellSize = EditorGUILayout.Slider("Preview Cell Size", _previewCellSize, 15f, 60f);
            _previewSpacing = EditorGUILayout.Slider("Preview Spacing", _previewSpacing, 0f, 10f);

            EditorGUILayout.Space(10);

            float totalWidth = _config.columns * (_previewCellSize + _previewSpacing);
            float totalHeight = _config.rows * (_previewCellSize + _previewSpacing);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(totalWidth + 20, totalHeight + 20);

            // Draw grid background
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, totalWidth + 10, totalHeight + 10),
                _config.visualConfig != null ? _config.visualConfig.gridBackgroundColor : new Color(0.15f, 0.15f, 0.15f));

            for (int row = 0; row < _config.rows; row++)
            {
                for (int col = 0; col < _config.columns; col++)
                {
                    var cellRect = new Rect(
                        rect.x + 5 + col * (_previewCellSize + _previewSpacing),
                        rect.y + 5 + row * (_previewCellSize + _previewSpacing),
                        _previewCellSize,
                        _previewCellSize
                    );

                    bool isActive = _config.IsCellActive(col, row);
                    Color cellColor = isActive
                        ? (_config.visualConfig != null ? _config.visualConfig.cellBackgroundColor : new Color(0.3f, 0.3f, 0.3f))
                        : new Color(0.1f, 0.1f, 0.1f, 0.3f);

                    EditorGUI.DrawRect(cellRect, cellColor);

                    if (isActive)
                    {
                        // Draw piece placeholder
                        float pieceMargin = _previewCellSize * 0.15f;
                        var pieceRect = new Rect(
                            cellRect.x + pieceMargin,
                            cellRect.y + pieceMargin,
                            cellRect.width - pieceMargin * 2,
                            cellRect.height - pieceMargin * 2
                        );
                        EditorGUI.DrawRect(pieceRect, new Color(0.5f, 0.7f, 1f, 0.6f));
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Info
            Vector2 gridSize = _config.GetGridPixelSize();
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Dimensions: {_config.columns} x {_config.rows}");
            EditorGUILayout.LabelField($"Pixel Size: {gridSize.x:F0} x {gridSize.y:F0}");
            EditorGUILayout.LabelField($"Active Cells: {CountActiveCells()}");
            EditorGUILayout.LabelField($"Interaction: {_config.interactionMode}");
            EditorGUILayout.LabelField($"Spawn Mode: {_config.spawnMode}");
        }

        #endregion

        #region Helpers

        private int CountActiveCells()
        {
            int count = 0;
            for (int col = 0; col < _config.columns; col++)
                for (int row = 0; row < _config.rows; row++)
                    if (_config.IsCellActive(col, row))
                        count++;
            return count;
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Grid Config", "GridConfig", "asset", "Choose location for the new Grid Config");

            if (string.IsNullOrEmpty(path)) return;

            var config = CreateInstance<GridConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            _config = config;
            EditorGUIUtility.PingObject(config);
        }

        private void CreateSubConfig<T>(string defaultName, string propertyName) where T : ScriptableObject
        {
            string configPath = AssetDatabase.GetAssetPath(_config);
            string directory = System.IO.Path.GetDirectoryName(configPath);

            string path = EditorUtility.SaveFilePanelInProject(
                $"Create {typeof(T).Name}", defaultName, "asset",
                $"Choose location for the {typeof(T).Name}",
                directory);

            if (string.IsNullOrEmpty(path)) return;

            var subConfig = CreateInstance<T>();
            AssetDatabase.CreateAsset(subConfig, path);
            AssetDatabase.SaveAssets();

            var so = new SerializedObject(_config);
            so.FindProperty(propertyName).objectReferenceValue = subConfig;
            so.ApplyModifiedProperties();

            EditorGUIUtility.PingObject(subConfig);
        }

        #endregion
    }
}
