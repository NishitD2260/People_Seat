#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace PeopleSeat.Gameplay.Editor
{
    /// <summary>
    /// Builds <see cref="SeatAuthoring"/> chains: bottom rows (BL then BR, low row index = near waiting),
    /// then top rows (TL then TR, low row index = near divider).
    /// <see cref="SeatAuthoring.AisleProgress"/> increases along that walk.
    /// </summary>
    public static class SeatingBlueprintSeatGenerator
    {
        public static void RebuildSeatsFromBlueprint(SeatingGridBlueprint bp, IList<SeatAuthoring> target)
        {
            if (bp == null || target == null) return;

            var b = Mathf.Max(0, bp.RowsBottom);
            var t = Mathf.Max(0, bp.RowsTop);
            var l = Mathf.Max(0, bp.ColsLeft);
            var r = Mathf.Max(0, bp.ColsRight);

            bp.EnsureListSizes();

            target.Clear();
            var aisleProgress = 0;

            for (var row = 0; row < b; row++)
            {
                if (l > 0)
                    AppendLeftBlockRow(bp.BottomLeft, row, l, target, ref aisleProgress);
                if (r > 0)
                    AppendRightBlockRow(bp.BottomRight, row, r, target, ref aisleProgress);
            }

            for (var row = 0; row < t; row++)
            {
                if (l > 0)
                    AppendLeftBlockRow(bp.TopLeft, row, l, target, ref aisleProgress);
                if (r > 0)
                    AppendRightBlockRow(bp.TopRight, row, r, target, ref aisleProgress);
            }

            for (var i = 0; i < target.Count; i++)
                target[i].SeatId = i;
        }

        private static void AppendLeftBlockRow(
            IReadOnlyList<PersonColor> cells,
            int row,
            int cols,
            IList<SeatAuthoring> target,
            ref int aisleProgress)
        {
            var towardAisleNeighborIndex = -1;
            for (var col = cols - 1; col >= 0; col--)
            {
                var depthFromAisle = cols - 1 - col;
                var color = cells[row * cols + col];
                towardAisleNeighborIndex = AddSeat(target, ref aisleProgress, towardAisleNeighborIndex, depthFromAisle, color);
            }
        }

        private static void AppendRightBlockRow(
            IReadOnlyList<PersonColor> cells,
            int row,
            int cols,
            IList<SeatAuthoring> target,
            ref int aisleProgress)
        {
            var towardAisleNeighborIndex = -1;
            for (var col = 0; col < cols; col++)
            {
                var depthFromAisle = col;
                var color = cells[row * cols + col];
                towardAisleNeighborIndex = AddSeat(target, ref aisleProgress, towardAisleNeighborIndex, depthFromAisle, color);
            }
        }

        private static int AddSeat(
            IList<SeatAuthoring> target,
            ref int aisleProgress,
            int towardAisleSeatIndex,
            int depthFromAisle,
            PersonColor color)
        {
            var idx = target.Count;
            target.Add(new SeatAuthoring
            {
                SeatId = idx,
                Color = color,
                AisleProgress = aisleProgress++,
                DepthFromAisle = depthFromAisle,
                TowardAisleSeatIndex = towardAisleSeatIndex,
            });
            return idx;
        }
    }
}
#endif
