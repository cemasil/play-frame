using UnityEngine;
using UnityEditor;

namespace PlayFrame.Systems.Layout.Editor
{
    /// <summary>
    /// Custom editor for GameLayoutConfig that shows a visual preview of the layout zones.
    /// </summary>
    [CustomEditor(typeof(GameLayoutConfig))]
    public class GameLayoutConfigEditor : UnityEditor.Editor
    {
        private const float PreviewHeight = 300f;
        private const float PreviewAspect = 9f / 16f; // Portrait phone

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Layout Preview", EditorStyles.boldLabel);

            var config = (GameLayoutConfig)target;
            DrawLayoutPreview(config);
        }

        private void DrawLayoutPreview(GameLayoutConfig config)
        {
            float previewWidth = PreviewHeight * PreviewAspect;
            var totalRect = GUILayoutUtility.GetRect(previewWidth + 40f, PreviewHeight + 20f);

            // Center the preview
            var previewRect = new Rect(
                totalRect.x + (totalRect.width - previewWidth) * 0.5f,
                totalRect.y + 10f,
                previewWidth,
                PreviewHeight
            );

            // Background
            EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.2f, 1f));

            // Draw zones
            DrawZone(previewRect, config.GetTopRect(), new Color(0.2f, 0.6f, 0.9f, 0.6f), "TOP HUD");
            DrawZone(previewRect, config.GetCenterRect(), new Color(0.3f, 0.8f, 0.3f, 0.4f), "GAME AREA");
            DrawZone(previewRect, config.GetBottomRect(), new Color(0.9f, 0.6f, 0.2f, 0.6f), "BOTTOM");
            DrawZone(previewRect, config.GetLeftRect(), new Color(0.8f, 0.3f, 0.8f, 0.6f), "LEFT");
            DrawZone(previewRect, config.GetRightRect(), new Color(0.8f, 0.3f, 0.8f, 0.6f), "RIGHT");

            // Border
            DrawBorder(previewRect, Color.white);
        }

        private void DrawZone(Rect parent, LayoutRect zone, Color color, string label)
        {
            if (zone.IsZero) return;

            // Convert from anchor space (Y-up) to GUI space (Y-down)
            var zoneRect = new Rect(
                parent.x + zone.xMin * parent.width,
                parent.y + (1f - zone.yMax) * parent.height,
                (zone.xMax - zone.xMin) * parent.width,
                (zone.yMax - zone.yMin) * parent.height
            );

            EditorGUI.DrawRect(zoneRect, color);
            DrawBorder(zoneRect, new Color(1f, 1f, 1f, 0.3f));

            // Label
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                normal = { textColor = Color.white },
                fontSize = 9,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(zoneRect, label, style);
        }

        private void DrawBorder(Rect rect, Color color)
        {
            float t = 1f;
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, t), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - t, rect.width, t), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, t, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - t, rect.y, t, rect.height), color);
        }
    }
}
