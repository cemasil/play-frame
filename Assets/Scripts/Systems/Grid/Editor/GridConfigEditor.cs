using UnityEngine;
using UnityEditor;

namespace PlayFrame.Systems.Grid.Editor
{
    /// <summary>
    /// Custom Inspector for GridConfig. Embeds the grid designer directly in the Inspector,
    /// replacing the separate Grid Designer window.
    /// </summary>
    [CustomEditor(typeof(GridConfig))]
    public class GridConfigEditor : UnityEditor.Editor
    {
        private GridConfig _config;
        private SerializedProperty _shape, _columns, _rows, _cellSize, _cellSpacing;
        private SerializedProperty _interactionMode, _multiSelectCount, _minChainLength, _allowDiagonals;
        private SerializedProperty _spawnMode, _spawnAnimDuration, _spawnStagger, _useSpawnEasing;
        private SerializedProperty _visualConfig, _audioConfig;
        private SerializedProperty _defaultCellInteraction;
        private SerializedProperty _pieceSprites, _useBoardLayout, _boardLayout;
        private SerializedProperty _customCellMask;

        private bool _showGrid = true;
        private bool _showInteraction = true;
        private bool _showSpawn = false;
        private bool _showVisuals = false;
        private bool _showAudio = false;
        private bool _showBoardLayout = false;
        private bool _showPreview = true;

        private float _previewCellSize = 30f;
        private int _selectedPieceType = 0;
        private bool _isErasing = false;

        // Board layout painting
        private enum PaintMode { None, Paint, Erase }
        private PaintMode _paintMode = PaintMode.None;

        private void OnEnable()
        {
            _config = (GridConfig)target;
            CacheProperties();
        }

        private void CacheProperties()
        {
            _shape = serializedObject.FindProperty("shape");
            _columns = serializedObject.FindProperty("columns");
            _rows = serializedObject.FindProperty("rows");
            _cellSize = serializedObject.FindProperty("cellSize");
            _cellSpacing = serializedObject.FindProperty("cellSpacing");
            _interactionMode = serializedObject.FindProperty("interactionMode");
            _multiSelectCount = serializedObject.FindProperty("multiSelectCount");
            _minChainLength = serializedObject.FindProperty("minChainLength");
            _allowDiagonals = serializedObject.FindProperty("allowDiagonals");
            _spawnMode = serializedObject.FindProperty("spawnMode");
            _spawnAnimDuration = serializedObject.FindProperty("spawnAnimationDuration");
            _spawnStagger = serializedObject.FindProperty("spawnStaggerDelay");
            _useSpawnEasing = serializedObject.FindProperty("useSpawnEasing");
            _visualConfig = serializedObject.FindProperty("visualConfig");
            _audioConfig = serializedObject.FindProperty("audioConfig");
            _defaultCellInteraction = serializedObject.FindProperty("defaultCellInteraction");
            _pieceSprites = serializedObject.FindProperty("pieceSprites");
            _useBoardLayout = serializedObject.FindProperty("useBoardLayout");
            _boardLayout = serializedObject.FindProperty("boardLayout");
            _customCellMask = serializedObject.FindProperty("customCellMask");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGridSection();
            DrawInteractionSection();
            DrawSpawnSection();
            DrawVisualsSection();
            DrawAudioSection();
            DrawBoardLayoutSection();
            DrawPreviewSection();

            serializedObject.ApplyModifiedProperties();
        }

        #region Grid Section

        private void DrawGridSection()
        {
            _showGrid = EditorGUILayout.BeginFoldoutHeaderGroup(_showGrid, "Grid Dimensions");
            if (_showGrid)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_shape);
                EditorGUILayout.PropertyField(_columns);

                if (_config.shape != GridShape.Square)
                    EditorGUILayout.PropertyField(_rows);

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(_cellSize);
                EditorGUILayout.PropertyField(_cellSpacing);
                EditorGUILayout.PropertyField(_defaultCellInteraction);

                Vector2 gridSize = _config.GetGridPixelSize();
                EditorGUILayout.HelpBox(
                    $"Grid Size: {gridSize.x:F0} x {gridSize.y:F0} px  |  " +
                    $"Total: {_config.columns * _config.rows} cells  |  " +
                    $"Active: {CountActiveCells()}",
                    MessageType.Info);

                if (_config.shape == GridShape.Custom)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Custom Cell Mask", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Click cells to toggle active/inactive", MessageType.None);
                    DrawCellMaskEditor();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawCellMaskEditor()
        {
            if (_config.customCellMask == null || _config.customCellMask.Length != _config.columns * _config.rows)
            {
                if (GUILayout.Button("Initialize Cell Mask"))
                {
                    Undo.RecordObject(_config, "Initialize Cell Mask");
                    _config.customCellMask = new bool[_config.columns * _config.rows];
                    for (int i = 0; i < _config.customCellMask.Length; i++)
                        _config.customCellMask[i] = true;
                    EditorUtility.SetDirty(_config);
                }
                return;
            }

            float cellSize = 22f;
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
                        cellSize, cellSize);

                    bool isActive = _config.customCellMask[index];
                    EditorGUI.DrawRect(cellRect, isActive ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.2f, 0.2f, 0.2f));

                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        Undo.RecordObject(_config, "Toggle Cell");
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
                Undo.RecordObject(_config, "Fill All Cells");
                for (int i = 0; i < _config.customCellMask.Length; i++)
                    _config.customCellMask[i] = true;
                EditorUtility.SetDirty(_config);
            }
            if (GUILayout.Button("Clear All"))
            {
                Undo.RecordObject(_config, "Clear All Cells");
                for (int i = 0; i < _config.customCellMask.Length; i++)
                    _config.customCellMask[i] = false;
                EditorUtility.SetDirty(_config);
            }
            if (GUILayout.Button("Invert"))
            {
                Undo.RecordObject(_config, "Invert Cells");
                for (int i = 0; i < _config.customCellMask.Length; i++)
                    _config.customCellMask[i] = !_config.customCellMask[i];
                EditorUtility.SetDirty(_config);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Interaction Section

        private void DrawInteractionSection()
        {
            _showInteraction = EditorGUILayout.BeginFoldoutHeaderGroup(_showInteraction, "Interaction");
            if (_showInteraction)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_interactionMode);

                if (_config.HasInteraction(GridInteractionMode.MultiSelect))
                {
                    EditorGUILayout.Space(3);
                    EditorGUILayout.PropertyField(_multiSelectCount);
                }

                if (_config.HasInteraction(GridInteractionMode.Chain) || _config.HasInteraction(GridInteractionMode.Draw))
                {
                    EditorGUILayout.Space(3);
                    EditorGUILayout.PropertyField(_minChainLength);
                    EditorGUILayout.PropertyField(_allowDiagonals);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Spawn Section

        private void DrawSpawnSection()
        {
            _showSpawn = EditorGUILayout.BeginFoldoutHeaderGroup(_showSpawn, "Spawn");
            if (_showSpawn)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spawnMode);
                EditorGUILayout.PropertyField(_spawnAnimDuration);
                EditorGUILayout.PropertyField(_spawnStagger);
                EditorGUILayout.PropertyField(_useSpawnEasing);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Visuals Section

        private void DrawVisualsSection()
        {
            _showVisuals = EditorGUILayout.BeginFoldoutHeaderGroup(_showVisuals, "Visuals");
            if (_showVisuals)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_visualConfig);

                if (_config.visualConfig != null)
                {
                    var visualSO = new SerializedObject(_config.visualConfig);
                    visualSO.Update();

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Background", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridBackground"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridBackgroundColor"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingLeft"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingRight"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingTop"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridPaddingBottom"));

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Border", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridBorderSprite"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("gridBorderColor"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("borderWidth"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("dynamicBorder"));

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Cells", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("cellBackground"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("cellBackgroundColor"));

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Selection", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("selectionColor"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("selectionScale"));

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Spawn Animation", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(visualSO.FindProperty("spawnStartScale"));
                    EditorGUILayout.PropertyField(visualSO.FindProperty("spawnStartAlpha"));

                    visualSO.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Audio Section

        private void DrawAudioSection()
        {
            _showAudio = EditorGUILayout.BeginFoldoutHeaderGroup(_showAudio, "Audio");
            if (_showAudio)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_audioConfig);

                if (_config.audioConfig != null)
                {
                    var audioSO = new SerializedObject(_config.audioConfig);
                    audioSO.Update();

                    if (_config.HasInteraction(GridInteractionMode.Tap) ||
                        _config.HasInteraction(GridInteractionMode.MultiSelect))
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("Tap/Select", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(audioSO.FindProperty("tapSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("deselectSound"));
                    }

                    if (_config.HasInteraction(GridInteractionMode.Swap))
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("Swap", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(audioSO.FindProperty("swapStartSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("swapSuccessSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("swapFailSound"));
                    }

                    if (_config.HasInteraction(GridInteractionMode.DragAndDrop))
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("Drag & Drop", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(audioSO.FindProperty("dragStartSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("dropSuccessSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("dropFailSound"));
                    }

                    if (_config.HasInteraction(GridInteractionMode.Chain) ||
                        _config.HasInteraction(GridInteractionMode.Draw))
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("Chain/Draw", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(audioSO.FindProperty("chainAddSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("chainRemoveSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("chainConfirmSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("chainFailSound"));
                    }

                    if (_config.HasInteraction(GridInteractionMode.MultiSelect))
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("Multi-Select", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(audioSO.FindProperty("multiSelectSound"));
                        EditorGUILayout.PropertyField(audioSO.FindProperty("multiSelectCompleteSound"));
                    }

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Match/Destroy", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(audioSO.FindProperty("matchSound"));
                    EditorGUILayout.PropertyField(audioSO.FindProperty("destroySound"));

                    audioSO.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Board Layout Section

        private void DrawBoardLayoutSection()
        {
            _showBoardLayout = EditorGUILayout.BeginFoldoutHeaderGroup(_showBoardLayout, "Board Layout Designer");
            if (_showBoardLayout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_pieceSprites, new GUIContent("Piece Sprites"));
                EditorGUILayout.PropertyField(_useBoardLayout, new GUIContent("Use Board Layout"));

                if (_config.useBoardLayout)
                {
                    int expectedSize = _config.columns * _config.rows;
                    if (_config.boardLayout == null || _config.boardLayout.Length != expectedSize)
                    {
                        if (GUILayout.Button("Initialize Board Layout"))
                        {
                            Undo.RecordObject(_config, "Initialize Board Layout");
                            _config.boardLayout = new int[expectedSize];
                            for (int i = 0; i < expectedSize; i++)
                                _config.boardLayout[i] = -1;
                            EditorUtility.SetDirty(_config);
                        }
                    }
                    else
                    {
                        DrawBoardLayoutDesigner();
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBoardLayoutDesigner()
        {
            bool hasSprites = _config.pieceSprites != null && _config.pieceSprites.Length > 0;

            // Piece palette
            if (hasSprites)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Piece Palette", EditorStyles.miniBoldLabel);

                float paletteSize = 36f;
                int spritesPerRow = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth - 60) / (int)(paletteSize + 4));

                EditorGUILayout.BeginHorizontal();

                // Eraser button
                var eraserStyle = _isErasing ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : GUI.skin.button;
                var eraserBg = GUI.backgroundColor;
                if (_isErasing) GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("X", eraserStyle, GUILayout.Width(paletteSize), GUILayout.Height(paletteSize)))
                {
                    _isErasing = true;
                    _selectedPieceType = -1;
                }
                GUI.backgroundColor = eraserBg;

                for (int i = 0; i < _config.pieceSprites.Length; i++)
                {
                    if ((i + 1) % spritesPerRow == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    bool isSelected = !_isErasing && _selectedPieceType == i;
                    var bg = GUI.backgroundColor;
                    if (isSelected) GUI.backgroundColor = Color.cyan;

                    var sprite = _config.pieceSprites[i];
                    if (sprite != null)
                    {
                        var tex = AssetPreview.GetAssetPreview(sprite);
                        if (tex != null)
                        {
                            if (GUILayout.Button(tex, GUILayout.Width(paletteSize), GUILayout.Height(paletteSize)))
                            {
                                _selectedPieceType = i;
                                _isErasing = false;
                            }
                        }
                        else
                        {
                            if (GUILayout.Button($"{i}", GUILayout.Width(paletteSize), GUILayout.Height(paletteSize)))
                            {
                                _selectedPieceType = i;
                                _isErasing = false;
                            }
                        }
                    }
                    else
                    {
                        if (GUILayout.Button($"{i}", GUILayout.Width(paletteSize), GUILayout.Height(paletteSize)))
                        {
                            _selectedPieceType = i;
                            _isErasing = false;
                        }
                    }
                    GUI.backgroundColor = bg;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);
                string modeLabel = _isErasing ? "Mode: Erase (click cell to clear)" : $"Mode: Paint piece type {_selectedPieceType}";
                EditorGUILayout.LabelField(modeLabel, EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("Add sprites to 'Piece Sprites' to enable visual painting. You can still set piece type IDs by clicking cells.", MessageType.Info);
            }

            // Grid painter
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Board Grid", EditorStyles.miniBoldLabel);

            float gridCellSize = Mathf.Clamp(
                (EditorGUIUtility.currentViewWidth - 60) / _config.columns - 2,
                20f, 50f);

            float totalWidth = _config.columns * (gridCellSize + 2);
            float totalHeight = _config.rows * (gridCellSize + 2);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

            for (int row = 0; row < _config.rows; row++)
            {
                for (int col = 0; col < _config.columns; col++)
                {
                    int index = row * _config.columns + col;
                    bool isActive = _config.IsCellActive(col, row);

                    var cellRect = new Rect(
                        rect.x + col * (gridCellSize + 2),
                        rect.y + row * (gridCellSize + 2),
                        gridCellSize, gridCellSize);

                    if (!isActive)
                    {
                        EditorGUI.DrawRect(cellRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
                        continue;
                    }

                    int pieceType = _config.boardLayout[index];

                    // Draw cell background
                    EditorGUI.DrawRect(cellRect, new Color(0.25f, 0.25f, 0.25f));

                    // Draw piece sprite or type number
                    if (pieceType >= 0)
                    {
                        bool drewSprite = false;
                        if (hasSprites && pieceType < _config.pieceSprites.Length && _config.pieceSprites[pieceType] != null)
                        {
                            var tex = AssetPreview.GetAssetPreview(_config.pieceSprites[pieceType]);
                            if (tex != null)
                            {
                                float margin = 2f;
                                var pieceRect = new Rect(
                                    cellRect.x + margin, cellRect.y + margin,
                                    cellRect.width - margin * 2, cellRect.height - margin * 2);
                                GUI.DrawTexture(pieceRect, tex, ScaleMode.ScaleToFit);
                                drewSprite = true;
                            }
                        }
                        if (!drewSprite)
                        {
                            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                            {
                                fontSize = 10,
                                normal = { textColor = Color.white }
                            };
                            GUI.Label(cellRect, pieceType.ToString(), style);
                        }
                    }

                    // Handle click
                    if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                        cellRect.Contains(Event.current.mousePosition))
                    {
                        Undo.RecordObject(_config, "Paint Board Layout");
                        _config.boardLayout[index] = _isErasing ? -1 : _selectedPieceType;
                        EditorUtility.SetDirty(_config);
                        Event.current.Use();
                        Repaint();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Utility buttons
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All"))
            {
                Undo.RecordObject(_config, "Clear Board Layout");
                for (int i = 0; i < _config.boardLayout.Length; i++)
                    _config.boardLayout[i] = -1;
                EditorUtility.SetDirty(_config);
            }
            if (hasSprites && GUILayout.Button("Fill Random"))
            {
                Undo.RecordObject(_config, "Fill Random Board");
                for (int i = 0; i < _config.boardLayout.Length; i++)
                {
                    int r = i / _config.columns;
                    int c = i % _config.columns;
                    _config.boardLayout[i] = _config.IsCellActive(c, r)
                        ? Random.Range(0, _config.pieceSprites.Length)
                        : -1;
                }
                EditorUtility.SetDirty(_config);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Preview Section

        private void DrawPreviewSection()
        {
            _showPreview = EditorGUILayout.BeginFoldoutHeaderGroup(_showPreview, "Preview");
            if (_showPreview)
            {
                _previewCellSize = EditorGUILayout.Slider("Cell Size", _previewCellSize, 15f, 60f);
                EditorGUILayout.Space(5);

                float spacing = 2f;
                float totalWidth = _config.columns * (_previewCellSize + spacing);
                float totalHeight = _config.rows * (_previewCellSize + spacing);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var rect = GUILayoutUtility.GetRect(totalWidth + 10, totalHeight + 10);

                // Grid background
                Color bgColor = _config.visualConfig != null
                    ? _config.visualConfig.gridBackgroundColor
                    : new Color(0.15f, 0.15f, 0.15f);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, totalWidth + 10, totalHeight + 10), bgColor);

                Color cellColor = _config.visualConfig != null
                    ? _config.visualConfig.cellBackgroundColor
                    : new Color(0.3f, 0.3f, 0.3f);

                for (int row = 0; row < _config.rows; row++)
                {
                    for (int col = 0; col < _config.columns; col++)
                    {
                        var cellRect = new Rect(
                            rect.x + 5 + col * (_previewCellSize + spacing),
                            rect.y + 5 + row * (_previewCellSize + spacing),
                            _previewCellSize, _previewCellSize);

                        bool isActive = _config.IsCellActive(col, row);
                        EditorGUI.DrawRect(cellRect, isActive ? cellColor : new Color(0.1f, 0.1f, 0.1f, 0.3f));

                        if (isActive)
                        {
                            float margin = _previewCellSize * 0.15f;
                            var pieceRect = new Rect(
                                cellRect.x + margin, cellRect.y + margin,
                                cellRect.width - margin * 2, cellRect.height - margin * 2);

                            // Show board layout piece if available
                            if (_config.useBoardLayout && _config.boardLayout != null)
                            {
                                int idx = row * _config.columns + col;
                                int pt = idx < _config.boardLayout.Length ? _config.boardLayout[idx] : -1;
                                if (pt >= 0 && _config.pieceSprites != null &&
                                    pt < _config.pieceSprites.Length && _config.pieceSprites[pt] != null)
                                {
                                    var tex = AssetPreview.GetAssetPreview(_config.pieceSprites[pt]);
                                    if (tex != null)
                                        GUI.DrawTexture(pieceRect, tex, ScaleMode.ScaleToFit);
                                    else
                                        EditorGUI.DrawRect(pieceRect, new Color(0.5f, 0.7f, 1f, 0.6f));
                                }
                                else
                                {
                                    EditorGUI.DrawRect(pieceRect, new Color(0.5f, 0.7f, 1f, 0.6f));
                                }
                            }
                            else
                            {
                                EditorGUI.DrawRect(pieceRect, new Color(0.5f, 0.7f, 1f, 0.6f));
                            }
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // Info
                Vector2 gridSize = _config.GetGridPixelSize();
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Pixel: {gridSize.x:F0}x{gridSize.y:F0}  |  Interaction: {_config.interactionMode}  |  Spawn: {_config.spawnMode}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
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

        #endregion
    }
}
