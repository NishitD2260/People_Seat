#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace PeopleSeat.Gameplay.Editor
{
    public class SeatingLevelEditorWindow : EditorWindow
    {
        private const float CellSize = 44f;

        private static Color SeatColorDisplay(PersonColor c)
        {
            switch (c)
            {
                case PersonColor.Red: return new Color(0.92f, 0.28f, 0.28f);
                case PersonColor.Blue: return new Color(0.22f, 0.48f, 0.95f);
                case PersonColor.Yellow: return new Color(0.98f, 0.88f, 0.22f);
                case PersonColor.Pink: return new Color(0.98f, 0.45f, 0.78f);
                case PersonColor.Green: return new Color(0.28f, 0.78f, 0.38f);
                case PersonColor.Orange: return new Color(0.98f, 0.52f, 0.14f);
                default: return Color.gray;
            }
        }

        private SeatingLevelSO _target;
        private Vector2 _scroll;
        private SerializedObject _serializedLevel;

        /// <summary>Click a palette swatch, then click seats to assign.</summary>
        private PersonColor _paintColor = PersonColor.Red;

        [MenuItem("People Seat/Seating Level Editor", priority = 10)]
        public static void Open()
        {
            var win = GetWindow<SeatingLevelEditorWindow>(true, "Seat level editor", true);
            win.minSize = new Vector2(560, 640);
        }

        public static void Open(SeatingLevelSO level)
        {
            Open();
            if (level != null)
                GetWindow<SeatingLevelEditorWindow>().SetTarget(level);
        }

        public void SetTarget(SeatingLevelSO level)
        {
            _target = level;
            _serializedLevel = level != null ? new SerializedObject(level) : null;
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is SeatingLevelSO so)
                SetTarget(so);
            Repaint();
        }

        private void OnEnable()
        {
            if (_target == null && Selection.activeObject is SeatingLevelSO selected)
                SetTarget(selected);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("People Seat — level authoring", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newTarget = (SeatingLevelSO)EditorGUILayout.ObjectField("Seating level", _target, typeof(SeatingLevelSO), false);
            if (EditorGUI.EndChangeCheck() && newTarget != _target)
                SetTarget(newTarget);

            if (_target == null)
            {
                EditorGUILayout.HelpBox("Assign a SeatingLevelSO asset, or select one in the Project window.", MessageType.Info);
                if (GUILayout.Button("Use currently selected asset") && Selection.activeObject is SeatingLevelSO so)
                    SetTarget(so);
                return;
            }

            if (_serializedLevel == null || _serializedLevel.targetObject != _target)
                _serializedLevel = new SerializedObject(_target);

            _serializedLevel.Update();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.PropertyField(_serializedLevel.FindProperty(nameof(SeatingLevelSO.WaitingAreaCapacity)));

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Lanes — front of lane = first in list", EditorStyles.boldLabel);
            DrawLanes(_serializedLevel.FindProperty(nameof(SeatingLevelSO.Lanes)));

            EditorGUILayout.Space(16);
            DrawSeatPaintingSection(_serializedLevel);

            EditorGUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Resize color lists to dimensions", GUILayout.Height(26)))
                {
                    Undo.RecordObject(_target, "Resize seat grid lists");
                    _target.GridBlueprint.EnsureListSizes();
                    EditorUtility.SetDirty(_target);
                    _serializedLevel.Update();
                }

                if (GUILayout.Button("Rebuild Seats from grid", GUILayout.Height(26)))
                {
                    Undo.RecordObject(_target, "Rebuild seats from blueprint");
                    _target.GridBlueprint.EnsureListSizes();
                    SeatingBlueprintSeatGenerator.RebuildSeatsFromBlueprint(_target.GridBlueprint, _target.Seats);
                    EditorUtility.SetDirty(_target);
                    AssetDatabase.SaveAssets();
                    _serializedLevel.Update();
                }
            }

            EditorGUILayout.HelpBox(
                "Row 0 in bottom quadrants = nearest waiting. Row 0 in top = nearest divider. " +
                "Left blocks: col 0 = farthest from aisle. Right blocks: col 0 = aisle edge.",
                MessageType.None);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Generated seats: {_target.Seats?.Count ?? 0}", EditorStyles.miniLabel);

            _serializedLevel.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void DrawSeatPaintingSection(SerializedObject levelSo)
        {
            EditorGUILayout.LabelField("Seat grid — paint mode", EditorStyles.boldLabel);

            DrawPaintPalette();

            var bp = levelSo.FindProperty(nameof(SeatingLevelSO.GridBlueprint));
            if (bp == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Grid size", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(bp.FindPropertyRelative(nameof(SeatingGridBlueprint.RowsBottom)), new GUIContent("Rows (bottom)"));
            EditorGUILayout.PropertyField(bp.FindPropertyRelative(nameof(SeatingGridBlueprint.RowsTop)), new GUIContent("Rows (top)"));
            EditorGUILayout.PropertyField(bp.FindPropertyRelative(nameof(SeatingGridBlueprint.ColsLeft)), new GUIContent("Cols (left of aisle)"));
            EditorGUILayout.PropertyField(bp.FindPropertyRelative(nameof(SeatingGridBlueprint.ColsRight)), new GUIContent("Cols (right of aisle)"));

            var rb = Mathf.Max(0, bp.FindPropertyRelative(nameof(SeatingGridBlueprint.RowsBottom)).intValue);
            var rt = Mathf.Max(0, bp.FindPropertyRelative(nameof(SeatingGridBlueprint.RowsTop)).intValue);
            var cl = Mathf.Max(0, bp.FindPropertyRelative(nameof(SeatingGridBlueprint.ColsLeft)).intValue);
            var cr = Mathf.Max(0, bp.FindPropertyRelative(nameof(SeatingGridBlueprint.ColsRight)).intValue);

            if (GUILayout.Button("Fit color lists to current size", GUILayout.Height(22)))
            {
                Undo.RecordObject(_target, "Fit grid lists");
                _target.GridBlueprint.EnsureListSizes();
                EditorUtility.SetDirty(_target);
                levelSo.Update();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Top half (row 0 = next to divider)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DrawQuadrantPaintable("Top-left", bp.FindPropertyRelative(nameof(SeatingGridBlueprint.TopLeft)), rt, cl, levelSo);
            DrawAisleColumn(" ", Mathf.Max(rt, 1));
            DrawQuadrantPaintable("Top-right", bp.FindPropertyRelative(nameof(SeatingGridBlueprint.TopRight)), rt, cr, levelSo);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            GUILayout.Label(
                "— divider / horizontal split —",
                new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic });

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Bottom half (row 0 = nearest waiting)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DrawQuadrantPaintable("Bottom-left", bp.FindPropertyRelative(nameof(SeatingGridBlueprint.BottomLeft)), rb, cl, levelSo);
            DrawAisleColumn("Aisle", Mathf.Max(rb, 1));
            DrawQuadrantPaintable("Bottom-right", bp.FindPropertyRelative(nameof(SeatingGridBlueprint.BottomRight)), rb, cr, levelSo);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAisleColumn(string label, int rowCount)
        {
            GUILayout.BeginVertical(GUILayout.Width(36));
            GUILayout.Space(22);
            var stripH = rowCount * (CellSize + 2f) + 18f;
            var r = GUILayoutUtility.GetRect(32f, stripH);
            EditorGUI.DrawRect(r, new Color(0.62f, 0.52f, 0.92f, 0.55f));
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1f), new Color(0.4f, 0.35f, 0.6f));
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1f, r.width, 1f), new Color(0.4f, 0.35f, 0.6f));

            var lr = new Rect(r.x, r.y, r.width, r.height);
            var gc = new GUIContent(string.IsNullOrEmpty(label) ? "║" : label);
            var st = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 9 };
            GUI.Label(lr, gc, st);
            GUILayout.EndVertical();
        }

        private void DrawPaintPalette()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("1. Choose color", EditorStyles.boldLabel);

            var names = Enum.GetNames(typeof(PersonColor));
            var vals = (PersonColor[])Enum.GetValues(typeof(PersonColor));
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < vals.Length; i++)
            {
                var pc = vals[i];
                var display = SeatColorDisplay(pc);
                var selected = _paintColor == pc;

                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = display;
                if (GUILayout.Button(names[i], EditorStyles.miniButton, GUILayout.MinHeight(36f), GUILayout.MinWidth(76f)))
                {
                    _paintColor = pc;
                    Repaint();
                }

                if (selected)
                {
                    var hl = GUILayoutUtility.GetLastRect();
                    const float t = 2f;
                    EditorGUI.DrawRect(new Rect(hl.x, hl.y, hl.width, t), Color.white);
                    EditorGUI.DrawRect(new Rect(hl.x, hl.yMax - t, hl.width, t), Color.white);
                    EditorGUI.DrawRect(new Rect(hl.x, hl.y, t, hl.height), Color.white);
                    EditorGUI.DrawRect(new Rect(hl.xMax - t, hl.y, t, hl.height), Color.white);
                }

                GUI.backgroundColor = oldBg;
            }

            EditorGUILayout.EndHorizontal();

            var sw = SeatColorDisplay(_paintColor);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active:", GUILayout.Width(48));
            var preview = GUILayoutUtility.GetRect(120, 22);
            EditorGUI.DrawRect(preview, sw);
            EditorGUI.DrawRect(new Rect(preview.x, preview.y, preview.width, 1f), Color.black);
            EditorGUI.DrawRect(new Rect(preview.x, preview.yMax - 1f, preview.width, 1f), Color.black);
            EditorGUI.DrawRect(new Rect(preview.x, preview.y, 1f, preview.height), Color.black);
            EditorGUI.DrawRect(new Rect(preview.xMax - 1f, preview.y, 1f, preview.height), Color.black);
            EditorGUILayout.LabelField(_paintColor.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("2. Click any seat below to paint it with the active color.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawQuadrantPaintable(
            string title,
            SerializedProperty cellsProp,
            int rows,
            int cols,
            SerializedObject _)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinWidth(120));

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (rows <= 0 || cols <= 0)
            {
                EditorGUILayout.LabelField("(set rows & cols > 0)", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            if (cellsProp == null || cellsProp.arraySize < rows * cols)
            {
                EditorGUILayout.HelpBox(
                    $"List length {cellsProp?.arraySize ?? 0} < {rows * cols}. Press \"Fit color lists to current size\".",
                    MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(26));
            for (var c = 0; c < cols; c++)
                GUILayout.Label($"c{c}", EditorStyles.miniLabel, GUILayout.Width(CellSize));
            EditorGUILayout.EndHorizontal();

            for (var r = rows - 1; r >= 0; r--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"r{r}", EditorStyles.miniLabel, GUILayout.Width(26));
                for (var c = 0; c < cols; c++)
                {
                    var idx = r * cols + c;
                    var sp = cellsProp.GetArrayElementAtIndex(idx);
                    var current = (PersonColor)sp.intValue;
                    var fill = SeatColorDisplay(current);
                    var isPaintMatch = current == _paintColor;

                    var oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = fill;

                    var tip = $"Current: {current}\nClick → set to {_paintColor}";
                    if (GUILayout.Button(new GUIContent(" ", tip), GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                    {
                        Undo.RecordObject(_target, $"Paint seat {title} [{r},{c}]");
                        sp.intValue = (int)_paintColor;
                        EditorUtility.SetDirty(_target);
                    }

                    GUI.backgroundColor = oldBg;

                    if (isPaintMatch)
                    {
                        var last = GUILayoutUtility.GetLastRect();
                        const float inset = 3f;
                        EditorGUI.DrawRect(
                            new Rect(last.xMin + inset, last.yMin + inset, last.width - inset * 2f, 2f),
                            new Color(1f, 1f, 1f, 0.85f));
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawLanes(SerializedProperty lanesProp)
        {
            EditorGUI.indentLevel++;
            for (var i = 0; i < lanesProp.arraySize; i++)
            {
                var lane = lanesProp.GetArrayElementAtIndex(i);
                var groups = lane.FindPropertyRelative(nameof(LaneDefinitionAuthoring.GroupsFrontToBack));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField($"Lane {i + 1}", EditorStyles.boldLabel);

                for (var g = 0; g < groups.arraySize; g++)
                {
                    var el = groups.GetArrayElementAtIndex(g);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(el.FindPropertyRelative(nameof(LaneGroupAuthoring.Color)), GUIContent.none, GUILayout.MinWidth(100));
                    EditorGUILayout.LabelField("Count", GUILayout.Width(40));
                    EditorGUILayout.PropertyField(el.FindPropertyRelative(nameof(LaneGroupAuthoring.Count)), GUIContent.none, GUILayout.Width(56));

                    if (GUILayout.Button("↑", GUILayout.Width(22)) && g > 0)
                    {
                        groups.MoveArrayElement(g, g - 1);
                        break;
                    }

                    if (GUILayout.Button("↓", GUILayout.Width(22)) && g < groups.arraySize - 1)
                    {
                        groups.MoveArrayElement(g, g + 1);
                        break;
                    }

                    if (GUILayout.Button("−", GUILayout.Width(22)))
                    {
                        groups.DeleteArrayElementAtIndex(g);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("+ Add group to lane"))
                    groups.InsertArrayElementAtIndex(groups.arraySize);
            }

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add lane"))
                    lanesProp.InsertArrayElementAtIndex(lanesProp.arraySize);

                if (GUILayout.Button("Ensure 3 lanes"))
                {
                    while (lanesProp.arraySize < 3)
                        lanesProp.InsertArrayElementAtIndex(lanesProp.arraySize);
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif
