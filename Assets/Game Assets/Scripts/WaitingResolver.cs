namespace PeopleSeat.Gameplay
{
    /// <summary>FIFO waiting queue: repeatedly seat the front person if any accessible match exists.</summary>
    public static class WaitingResolver
    {
        public static void Resolve(GridState grid, WaitingAreaState waiting)
        {
            while (waiting.TryPeekFront(out var color))
            {
                var preview = PlacementService.PreviewPlace(grid, color, 1);
                if (preview.SeatedCount == 0) break;

                PlacementService.ApplySeating(grid, preview.SeatIndicesInWalkOrder);
                waiting.TryDequeueFront(out _);
            }
        }
    }
}
