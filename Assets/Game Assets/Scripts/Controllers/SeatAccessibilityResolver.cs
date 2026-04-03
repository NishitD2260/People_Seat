using System.Collections.Generic;

public static class SeatAccessibilityResolver
{
    public static List<Seat> FindAccessibleSeats(GridManager grid, SeatColor color, int maxCount)
    {
        List<Seat> result = new List<Seat>();

        for (int row = grid.RowCount - 1; row >= 0; row--)
        {
            CheckSide(grid, row, GridSide.Left, color, maxCount, result);
            if (result.Count >= maxCount) return result;

            CheckSide(grid, row, GridSide.Right, color, maxCount, result);
            if (result.Count >= maxCount) return result;
        }

        return result;
    }

    private static void CheckSide(GridManager grid, int row, GridSide side, SeatColor color, int maxCount, List<Seat> result)
    {
        // Scan from FAR EDGE inward toward aisle.
        // In the instantiated grid, aisleDistance=0 is farthest from aisle on both sides.
        for (int d = 0; d < grid.SeatsPerSide; d++)
        {
            Seat seat = grid.GetSeat(row, side, d);
            if (seat == null) continue;

            if (seat.IsBlocked)
                break;

            if (seat.IsOccupied || seat.HasWalkIncoming)
                continue; // filled or someone already walking here — check next inward

            // First unoccupied seat from the far edge
            if (seat.Color == color)
            {
                result.Add(seat);
                if (result.Count >= maxCount) return;
            }

            break; // only the outermost empty seat is accessible per row-side
        }
    }

    public static int CountAccessibleSeats(GridManager grid, SeatColor color)
    {
        return FindAccessibleSeats(grid, color, int.MaxValue).Count;
    }

    public static bool HasAnyAccessibleSeat(GridManager grid, SeatColor color)
    {
        return FindAccessibleSeats(grid, color, 1).Count > 0;
    }
}
