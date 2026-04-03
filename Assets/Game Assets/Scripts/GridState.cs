using System;
using System.Collections.Generic;

namespace PeopleSeat.Gameplay
{
    /// <summary>Runtime seat: horizontal-from-aisle chain + aisle walk ordering.</summary>
    [Serializable]
    public struct SeatRuntime
    {
        public int Id;
        public PersonColor Color;
        public bool Occupied;
        /// <summary>Index of neighbor one step toward aisle; -1 if this seat is aisle-adjacent (edge).</summary>
        public int TowardAisleSeatIndex;
        public int AisleProgress;
        public int DepthFromAisle;
    }

    public sealed class GridState
    {
        public List<SeatRuntime> Seats { get; }

        public GridState(IReadOnlyList<SeatRuntime> initial)
        {
            Seats = new List<SeatRuntime>(initial);
        }

        public bool IsAccessible(int seatIndex)
        {
            var s = Seats[seatIndex];
            if (s.Occupied) return false;
            if (s.TowardAisleSeatIndex < 0) return true;
            return Seats[s.TowardAisleSeatIndex].Occupied;
        }

        public void SetOccupied(int seatIndex, bool occupied)
        {
            var s = Seats[seatIndex];
            s.Occupied = occupied;
            Seats[seatIndex] = s;
        }

        public static List<SeatRuntime> BuildFromAuthoring(IReadOnlyList<SeatAuthoring> definitions)
        {
            var list = new List<SeatRuntime>(definitions.Count);
            for (var i = 0; i < definitions.Count; i++)
            {
                var d = definitions[i];
                list.Add(new SeatRuntime
                {
                    Id = d.SeatId,
                    Color = d.Color,
                    Occupied = false,
                    TowardAisleSeatIndex = d.TowardAisleSeatIndex,
                    AisleProgress = d.AisleProgress,
                    DepthFromAisle = d.DepthFromAisle,
                });
            }

            return list;
        }
    }
}
