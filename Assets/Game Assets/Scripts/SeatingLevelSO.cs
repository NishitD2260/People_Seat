using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeopleSeat.Gameplay
{
    [Serializable]
    public class SeatAuthoring
    {
        [Tooltip("Stable id for tie-breaks and debugging.")]
        public int SeatId;

        public PersonColor Color;

        [Tooltip("Order along center aisle path (lower = encountered first when walking from lanes).")]
        public int AisleProgress;

        [Tooltip("0 = aisle-adjacent edge; increase for each step away from aisle in the row.")]
        public int DepthFromAisle;

        [Tooltip("Index in the Seats list (same order as this collection) of the neighbor one step toward the aisle, or -1 if aisle-adjacent.")]
        public int TowardAisleSeatIndex = -1;
    }

    [Serializable]
    public class LaneGroupAuthoring
    {
        public PersonColor Color;
        [Min(1)] public int Count = 1;
    }

    [CreateAssetMenu(fileName = "SeatingLevel", menuName = "People Seat/Seating Level", order = 0)]
    public class SeatingLevelSO : ScriptableObject
    {
        [Min(1)] public int WaitingAreaCapacity = SeatingGameRules.DefaultWaitingAreaCapacity;

        [Tooltip("Paint quadrants here, then Rebuild Seats in the Level Editor window.")]
        public SeatingGridBlueprint GridBlueprint = new SeatingGridBlueprint();

        public List<SeatAuthoring> Seats = new List<SeatAuthoring>();

        [Tooltip("One entry per lane, front-to-back (index 0 = tap front).")]
        public List<LaneDefinitionAuthoring> Lanes = new List<LaneDefinitionAuthoring>();
    }

    [Serializable]
    public class LaneDefinitionAuthoring
    {
        public List<LaneGroupAuthoring> GroupsFrontToBack = new List<LaneGroupAuthoring>();
    }
}
