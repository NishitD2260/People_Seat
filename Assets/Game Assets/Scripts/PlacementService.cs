using System.Collections.Generic;

namespace PeopleSeat.Gameplay
{
    public readonly struct PlacementOutcome
    {
        public readonly int SeatedCount;
        public readonly int ToWaitingCount;
        public readonly IReadOnlyList<int> SeatIndicesInWalkOrder;

        public PlacementOutcome(int seatedCount, int toWaitingCount, IReadOnlyList<int> seatIndicesInWalkOrder)
        {
            SeatedCount = seatedCount;
            ToWaitingCount = toWaitingCount;
            SeatIndicesInWalkOrder = seatIndicesInWalkOrder;
        }
    }

    public static class PlacementService
    {
        /// <summary>
        /// Selects up to <paramref name="count"/> accessible matching seats by aisle walk order.
        /// Does not mutate <paramref name="grid"/>; caller applies occupancy and waiting.
        /// </summary>
        public static PlacementOutcome PreviewPlace(GridState grid, PersonColor color, int count)
        {
            if (count <= 0)
                return new PlacementOutcome(0, 0, System.Array.Empty<int>());

            var eligible = new List<int>();
            for (var i = 0; i < grid.Seats.Count; i++)
            {
                var s = grid.Seats[i];
                if (s.Occupied || s.Color != color) continue;
                if (!grid.IsAccessible(i)) continue;
                eligible.Add(i);
            }

            eligible.Sort(CompareSeatOrder(grid));

            var take = System.Math.Min(count, eligible.Count);
            var chosen = eligible.GetRange(0, take);
            var toWait = count - take;

            return new PlacementOutcome(take, toWait, chosen);
        }

        /// <summary>Marks chosen seats occupied. Call after validating waiting capacity.</summary>
        public static void ApplySeating(GridState grid, IReadOnlyList<int> seatIndices)
        {
            for (var i = 0; i < seatIndices.Count; i++)
                grid.SetOccupied(seatIndices[i], true);
        }

        private static System.Comparison<int> CompareSeatOrder(GridState grid)
        {
            return (a, b) =>
            {
                var sa = grid.Seats[a];
                var sb = grid.Seats[b];
                var c = sa.AisleProgress.CompareTo(sb.AisleProgress);
                if (c != 0) return c;
                c = sb.DepthFromAisle.CompareTo(sa.DepthFromAisle);
                if (c != 0) return c;
                return sa.Id.CompareTo(sb.Id);
            };
        }
    }
}
