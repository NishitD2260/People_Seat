#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PeopleSeat.Gameplay.Editor
{
    public static class SeatingLevelMenu
    {
        private const string SamplePath = "Assets/Game Assets/LevelSo/SeatingLevel-Sample.asset";

        [MenuItem("People Seat/Create Sample Seating Level Asset")]
        public static void CreateSample()
        {
            var level = ScriptableObject.CreateInstance<SeatingLevelSO>();
            level.WaitingAreaCapacity = 10;

            // Two rows, one color each: bottom aisleProgress=0, top=100. Seat 1 edge blue depth 0, seat 2 inner blue depth 1.
            level.Seats.Add(new SeatAuthoring
            {
                SeatId = 0,
                Color = PersonColor.Blue,
                AisleProgress = 100,
                DepthFromAisle = 0,
                TowardAisleSeatIndex = -1,
            });
            level.Seats.Add(new SeatAuthoring
            {
                SeatId = 1,
                Color = PersonColor.Blue,
                AisleProgress = 100,
                DepthFromAisle = 1,
                TowardAisleSeatIndex = 0,
            });
            level.Seats.Add(new SeatAuthoring
            {
                SeatId = 2,
                Color = PersonColor.Red,
                AisleProgress = 0,
                DepthFromAisle = 0,
                TowardAisleSeatIndex = -1,
            });
            level.Seats.Add(new SeatAuthoring
            {
                SeatId = 3,
                Color = PersonColor.Red,
                AisleProgress = 0,
                DepthFromAisle = 1,
                TowardAisleSeatIndex = 2,
            });

            level.Lanes.Add(new LaneDefinitionAuthoring
            {
                GroupsFrontToBack = new List<LaneGroupAuthoring>
                {
                    new LaneGroupAuthoring { Color = PersonColor.Blue, Count = 2 },
                    new LaneGroupAuthoring { Color = PersonColor.Red, Count = 1 },
                },
            });
            level.Lanes.Add(new LaneDefinitionAuthoring
            {
                GroupsFrontToBack = new List<LaneGroupAuthoring>
                {
                    new LaneGroupAuthoring { Color = PersonColor.Red, Count = 2 },
                },
            });

            AssetDatabase.CreateAsset(level, SamplePath);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(level);
        }
    }
}
#endif
