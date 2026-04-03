using System;
using UnityEngine;

namespace PeopleSeat.Gameplay
{
    /// <summary>Owns grid, lanes, waiting; applies plan rules and exposes win/lose helpers.</summary>
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private SeatingLevelSO level;
        public SeatingLevelSO Level => level;

        public GridState Grid { get; private set; }
        public LaneState Lanes { get; private set; }
        public WaitingAreaState Waiting { get; private set; }

        public event Action<int, PlacementOutcome, int> OnPlacementResolved;
        public event Action<int, PlacementOutcome, int> OnPlacementRejected;

        private void Awake()
        {
            if (level != null)
                InitializeFromLevel(level);
        }

        public void InitializeFromLevel(SeatingLevelSO seatingLevel)
        {
            level = seatingLevel;
            if (seatingLevel == null)
            {
                Debug.LogWarning($"{nameof(GameFlowController)}: no SeatingLevelSO assigned.");
                return;
            }
 
            var seats = GridState.BuildFromAuthoring(seatingLevel.Seats);
            Grid = new GridState(seats);
            Lanes = new LaneState(seatingLevel.Lanes);
            Waiting = new WaitingAreaState(Mathf.Max(1, seatingLevel.WaitingAreaCapacity));
        }

        /// <returns>False if tap blocked (empty lane, or waiting overflow per rules).</returns>
        public bool TryTapLane(int laneIndex)
        {
            if (Grid == null || Lanes == null || Waiting == null) return false;
            if (!Lanes.HasGroup(laneIndex)) return false;

            var group = Lanes.PeekFront(laneIndex)!.Value;
            var preview = PlacementService.PreviewPlace(Grid, group.Color, group.Count);

            if (preview.ToWaitingCount > 0 && !Waiting.CanAccept(preview.ToWaitingCount))
            {
                OnPlacementRejected?.Invoke(laneIndex, preview, preview.ToWaitingCount);
                return false;
            }

            PlacementService.ApplySeating(Grid, preview.SeatIndicesInWalkOrder);
            if (preview.ToWaitingCount > 0)
                Waiting.EnqueueManySame(group.Color, preview.ToWaitingCount);

            Lanes.DequeueFront(laneIndex);

            WaitingResolver.Resolve(Grid, Waiting);

            OnPlacementResolved?.Invoke(laneIndex, preview, preview.ToWaitingCount);
            return true;
        }

        public bool IsWinState() =>
            Lanes != null && Waiting != null && Lanes.AllLanesEmpty() && Waiting.Count == 0;

        /// <summary>Heuristic fail: waiting full and front of every occupied lane cannot place anyone.</summary>
        public bool IsSoftLocked()
        {
            if (Waiting == null || Lanes == null || Grid == null) return false;
            if (Waiting.Count < Waiting.Capacity) return false;

            for (var i = 0; i < Lanes.Lanes.Count; i++)
            {
                var front = Lanes.PeekFront(i);
                if (!front.HasValue) continue;
                var p = PlacementService.PreviewPlace(Grid, front.Value.Color, front.Value.Count);
                if (p.SeatedCount > 0) return false;
            }

            return true;
        }
    }
}
