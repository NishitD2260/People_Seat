#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PeopleSeatLevelEditor : EditorWindow
{
    private LevelSO selectedLevel;
    private Vector2 scrollPos;
    private int selectedTab;
    private readonly string[] tabNames = { "Grid", "Lanes", "Settings" };

    // Grid painting
    private SeatColor paintColor = SeatColor.Red;
    private bool paintBlocked;

    // Color mapping for preview
    private static readonly Dictionary<SeatColor, Color> colorMap = new Dictionary<SeatColor, Color>
    {
        { SeatColor.Red, Color.red },
        { SeatColor.Blue, Color.blue },
        { SeatColor.Green, Color.green },
        { SeatColor.Yellow, Color.yellow },
        { SeatColor.Orange, new Color(1f, 0.5f, 0f) },
        { SeatColor.Magenta, Color.magenta },
    };

    [MenuItem("Tools/People Seat Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<PeopleSeatLevelEditor>("Level Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        selectedLevel = (LevelSO)EditorGUILayout.ObjectField("Level SO", selectedLevel, typeof(LevelSO), false);

        if (selectedLevel == null)
        {
            EditorGUILayout.HelpBox("Select a LevelSO asset to edit.", MessageType.Info);
            return;
        }

        // Ensure config exists
        if (selectedLevel.PeopleSeatConfig == null)
            selectedLevel.PeopleSeatConfig = new PeopleSeatLevelConfig();

        EditorGUILayout.Space(5);
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        EditorGUILayout.Space(5);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        switch (selectedTab)
        {
            case 0: DrawGridTab(); break;
            case 1: DrawLanesTab(); break;
            case 2: DrawSettingsTab(); break;
        }

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(selectedLevel);
        }
    }

    // ─────────────────────────────────────────────────────────
    // SETTINGS TAB
    // ─────────────────────────────────────────────────────────
    private void DrawSettingsTab()
    {
        var config = selectedLevel.PeopleSeatConfig;
        var grid = config.gridData;

        EditorGUILayout.LabelField("Grid Dimensions", EditorStyles.boldLabel);
        grid.totalRows = EditorGUILayout.IntField("Total Rows", grid.totalRows);
        grid.seatsPerSide = EditorGUILayout.IntField("Seats Per Side", grid.seatsPerSide);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Waiting Area", EditorStyles.boldLabel);
        config.waitingAreaCapacity = EditorGUILayout.IntField("Capacity", config.waitingAreaCapacity);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Dividers (row indices after which to place a divider)", EditorStyles.boldLabel);
        if (grid.dividerAfterRows == null) grid.dividerAfterRows = new List<int>();

        for (int i = 0; i < grid.dividerAfterRows.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            grid.dividerAfterRows[i] = EditorGUILayout.IntField($"Divider {i}", grid.dividerAfterRows[i]);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                grid.dividerAfterRows.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Divider"))
        {
            grid.dividerAfterRows.Add(0);
        }
    }

    // ─────────────────────────────────────────────────────────
    // GRID TAB
    // ─────────────────────────────────────────────────────────
    private void DrawGridTab()
    {
        var config = selectedLevel.PeopleSeatConfig;
        var grid = config.gridData;

        if (grid.totalRows == 0 || grid.seatsPerSide == 0)
        {
            EditorGUILayout.HelpBox("Set grid dimensions in the Settings tab first.", MessageType.Warning);
            return;
        }

        // Paint controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Paint Color:", GUILayout.Width(80));
        paintColor = (SeatColor)EditorGUILayout.EnumPopup(paintColor, GUILayout.Width(100));
        paintBlocked = EditorGUILayout.ToggleLeft("Blocked", paintBlocked, GUILayout.Width(70));

        if (GUILayout.Button("Fill All", GUILayout.Width(60)))
        {
            FillAllSeats(grid);
        }
        if (GUILayout.Button("Clear All", GUILayout.Width(60)))
        {
            grid.seats.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Build seat lookup
        Dictionary<(int, int), SeatEntry> lookup = new Dictionary<(int, int), SeatEntry>();
        if (grid.seats != null)
        {
            foreach (var s in grid.seats)
                lookup[(s.row, s.col)] = s;
        }

        int totalCols = grid.seatsPerSide * 2;

        // Draw grid
        EditorGUILayout.LabelField("Left Side          |  Aisle  |          Right Side", EditorStyles.centeredGreyMiniLabel);

        float cellSize = 30f;
        for (int row = 0; row < grid.totalRows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Left seats (col 0 = far left)
            for (int col = 0; col < grid.seatsPerSide; col++)
            {
                DrawSeatCell(grid, lookup, row, col, cellSize);
            }

            // Aisle indicator
            GUILayout.Space(10);
            EditorGUILayout.LabelField("|", GUILayout.Width(10), GUILayout.Height(cellSize));
            GUILayout.Space(10);

            // Right seats
            for (int col = grid.seatsPerSide; col < totalCols; col++)
            {
                DrawSeatCell(grid, lookup, row, col, cellSize);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Divider line
            if (grid.dividerAfterRows != null && grid.dividerAfterRows.Contains(row))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Color prev = GUI.color;
                GUI.color = Color.gray;
                GUILayout.Box("", GUILayout.Width(totalCols * (cellSize + 4) + 30), GUILayout.Height(4));
                GUI.color = prev;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DrawSeatCell(GridData grid, Dictionary<(int, int), SeatEntry> lookup, int row, int col, float cellSize)
    {
        bool hasEntry = lookup.ContainsKey((row, col));
        SeatEntry entry = hasEntry ? lookup[(row, col)] : null;

        Color prevBg = GUI.backgroundColor;

        if (entry != null && !entry.isBlocked)
        {
            GUI.backgroundColor = colorMap.ContainsKey(entry.color) ? colorMap[entry.color] : Color.white;
        }
        else if (entry != null && entry.isBlocked)
        {
            GUI.backgroundColor = Color.gray;
        }
        else
        {
            GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
        }

        string label = entry != null ? (entry.isBlocked ? "X" : entry.color.ToString()[0].ToString()) : "";

        if (GUILayout.Button(label, GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
        {
            // Toggle: if clicking same color, remove. Otherwise set.
            if (entry != null && entry.color == paintColor && entry.isBlocked == paintBlocked)
            {
                grid.seats.Remove(entry);
            }
            else
            {
                if (entry != null) grid.seats.Remove(entry);
                grid.seats.Add(new SeatEntry
                {
                    row = row,
                    col = col,
                    color = paintColor,
                    isBlocked = paintBlocked
                });
            }
        }

        GUI.backgroundColor = prevBg;
    }

    private void FillAllSeats(GridData grid)
    {
        grid.seats.Clear();
        int totalCols = grid.seatsPerSide * 2;
        for (int row = 0; row < grid.totalRows; row++)
        {
            for (int col = 0; col < totalCols; col++)
            {
                grid.seats.Add(new SeatEntry
                {
                    row = row,
                    col = col,
                    color = paintColor,
                    isBlocked = paintBlocked
                });
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    // LANES TAB
    // ─────────────────────────────────────────────────────────
    private void DrawLanesTab()
    {
        var config = selectedLevel.PeopleSeatConfig;
        if (config.lanes == null) config.lanes = new List<LaneData>();

        // Ensure 3 lanes
        while (config.lanes.Count < 3)
            config.lanes.Add(new LaneData());

        EditorGUILayout.BeginHorizontal();

        for (int laneIdx = 0; laneIdx < 3; laneIdx++)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MinWidth(150));
            EditorGUILayout.LabelField($"Lane {laneIdx + 1}", EditorStyles.boldLabel);

            LaneData lane = config.lanes[laneIdx];
            if (lane.groups == null) lane.groups = new List<PeopleGroupData>();

            for (int g = 0; g < lane.groups.Count; g++)
            {
                EditorGUILayout.BeginVertical("helpbox");
                string groupLabel = g == 0 ? "TOP (Tappable)" : $"Group {g}";
                EditorGUILayout.LabelField(groupLabel, EditorStyles.miniLabel);

                lane.groups[g].color = (SeatColor)EditorGUILayout.EnumPopup("Color", lane.groups[g].color);
                lane.groups[g].count = EditorGUILayout.IntField("Count", lane.groups[g].count);

                EditorGUILayout.BeginHorizontal();
                if (g > 0 && GUILayout.Button("Up", EditorStyles.miniButton))
                {
                    var temp = lane.groups[g];
                    lane.groups[g] = lane.groups[g - 1];
                    lane.groups[g - 1] = temp;
                }
                if (g < lane.groups.Count - 1 && GUILayout.Button("Down", EditorStyles.miniButton))
                {
                    var temp = lane.groups[g];
                    lane.groups[g] = lane.groups[g + 1];
                    lane.groups[g + 1] = temp;
                }
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    lane.groups.RemoveAt(g);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (GUILayout.Button("+ Add Group"))
            {
                lane.groups.Add(new PeopleGroupData { color = SeatColor.Red, count = 3 });
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();

        // Summary
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);

        int totalPeople = 0;
        int totalSeats = config.gridData.seats != null ? config.gridData.seats.FindAll(s => !s.isBlocked).Count : 0;
        foreach (var lane in config.lanes)
        {
            if (lane.groups == null) continue;
            foreach (var g in lane.groups)
                totalPeople += g.count;
        }

        EditorGUILayout.LabelField($"Total People: {totalPeople}");
        EditorGUILayout.LabelField($"Total Seats: {totalSeats}");
        EditorGUILayout.LabelField($"Waiting Capacity: {config.waitingAreaCapacity}");

        if (totalPeople > totalSeats + config.waitingAreaCapacity)
        {
            EditorGUILayout.HelpBox("Warning: More people than seats + waiting capacity!", MessageType.Warning);
        }
    }
}
#endif
