using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum GameState
{
    Initializing,
    Playing,
    Animating,
    Won,
    Lost
}

public class GameManager : SingletonBase<GameManager>
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LaneManager laneManager;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private MovementController movementController;
    [SerializeField] private ColorThemeConfig colorThemeConfig;

    public GameState CurrentState { get; private set; } = GameState.Initializing;

    /// <summary>Lane tap batches still running (group to seats and/or group to waiting).</summary>
    private int pendingLaneTapBatches;

    /// <summary>Waiting-area people currently walking to a seat.</summary>
    private int pendingWaitingToSeat;

    protected override void Awake()
    {
        DNDOnLoad = false;
        base.Awake();
    }

    private void OnEnable()
    {
        EventController.StartListening(GameEvent.EVENT_SET_LEVEL_DATA, OnLevelDataReceived);
        EventController.StartListening(GameEvent.EVENT_PEOPLE_GROUP_TAPPED, OnPeopleGroupTapped);
        EventController.StartListening(GameEvent.EVENT_PEOPLE_SEATED, OnSomeoneSeated);
    }

    private void OnDisable()
    {
        EventController.StopListening(GameEvent.EVENT_SET_LEVEL_DATA, OnLevelDataReceived);
        EventController.StopListening(GameEvent.EVENT_PEOPLE_GROUP_TAPPED, OnPeopleGroupTapped);
        EventController.StopListening(GameEvent.EVENT_PEOPLE_SEATED, OnSomeoneSeated);
    }

    private void OnLevelDataReceived(object args)
    {
        LevelSO levelSO = args as LevelSO;
        if (levelSO == null || levelSO.PeopleSeatConfig == null) return;

        PeopleSeatLevelConfig config = levelSO.PeopleSeatConfig;

        gridManager.BuildGrid(config.gridData, colorThemeConfig);
        laneManager.BuildLanes(config.lanes, colorThemeConfig);
        waitingAreaManager.Initialize(config.waitingAreaCapacity);
        movementController.Initialize(gridManager);
        RefreshSeatAccessibilityVisuals();

        CurrentState = GameState.Playing;
    }

    private void OnPeopleGroupTapped(object args)
    {
        if (CurrentState != GameState.Playing) return;

        PeopleGroupTapData data = args as PeopleGroupTapData;
        if (data == null) return;

        CurrentState = GameState.Animating;

        // Find accessible seats of matching color
        List<Seat> accessibleSeats = SeatAccessibilityResolver.FindAccessibleSeats(gridManager, data.Color, data.Count);

        int toSeat = accessibleSeats.Count;
        int toWaiting = data.Count - toSeat;

        Vector3 groupStartPos = data.Source.transform.position;
        pendingLaneTapBatches = 0;

        int seatingAnimCount = 0;
        int waitingAnimCount = 0;

        // People who will be seated
        if (toSeat > 0)
        {
            List<PersonView> seatingPeople = new List<PersonView>();
            for (int i = 0; i < toSeat; i++)
            {
                PersonView person = data.Source.ReleaseTopPerson();
                if (person != null) seatingPeople.Add(person);
            }

            seatingAnimCount = seatingPeople.Count;
            if (seatingAnimCount > 0)
            {
                pendingLaneTapBatches++;
                movementController.AnimateGroupToSeats(seatingPeople, accessibleSeats, groupStartPos, () =>
                {
                    AudioManager.Instance?.PlayAudio(AudioID.PEOPLE_SIT);
                    OnLaneTapBatchComplete();
                });
            }
        }

        // People who go to waiting area
        if (toWaiting > 0)
        {
            List<PersonView> waitingPeople = new List<PersonView>();
            for (int i = 0; i < toWaiting; i++)
            {
                PersonView person = data.Source.ReleaseTopPerson();
                if (person != null) waitingPeople.Add(person);
            }

            waitingAnimCount = waitingPeople.Count;
            if (waitingAnimCount > 0)
            {
                pendingLaneTapBatches++;
                movementController.AnimateGroupToWaiting(waitingPeople, waitingAreaManager, groupStartPos, () =>
                {
                    AudioManager.Instance?.PlayAudio(AudioID.PEOPLE_TO_WAITING);
                    OnLaneTapBatchComplete();
                });
            }
        }

        // Lane advances only after the last person from this tap has started moving (matches MovementController stagger).
        if (pendingLaneTapBatches == 0)
        {
            laneManager.RemoveTopGroup(data.LaneIndex);
            RefreshSeatAccessibilityVisuals();
            CurrentState = GameState.Playing;
            CheckWinLose();
        }
        else
        {
            float stagger = movementController.StaggerDelay;
            float lastStartDelay = 0f;
            if (seatingAnimCount > 0)
                lastStartDelay = Mathf.Max(lastStartDelay, (seatingAnimCount - 1) * stagger);
            if (waitingAnimCount > 0)
                lastStartDelay = Mathf.Max(lastStartDelay, (waitingAnimCount - 1) * stagger);

            int laneIndex = data.LaneIndex;
            DOVirtual.DelayedCall(lastStartDelay, () => laneManager.RemoveTopGroup(laneIndex))
                .SetLink(gameObject);
        }
    }

    private void OnLaneTapBatchComplete()
    {
        pendingLaneTapBatches--;
        if (pendingLaneTapBatches > 0) return;

        TryPlaceWaitingPeople();
        MaybeSettleIfIdle();
    }

    private void TryPlaceWaitingPeople()
    {
        List<SeatColor> waitingColors = waitingAreaManager.GetDistinctWaitingColors();
        bool anyPlaced = false;

        foreach (SeatColor color in waitingColors)
        {
            int waitingCount = waitingAreaManager.CountPeopleOfColor(color);
            if (waitingCount == 0) continue;

            List<Seat> seats = SeatAccessibilityResolver.FindAccessibleSeats(gridManager, color, waitingCount);
            if (seats.Count == 0) continue;

            anyPlaced = true;
            for (int i = 0; i < seats.Count; i++)
            {
                PersonView person = waitingAreaManager.RemovePerson(color);
                if (person == null) break;

                pendingWaitingToSeat++;
                Seat targetSeat = seats[i];
                movementController.AnimateWaitingToSeat(person, targetSeat, () =>
                {
                    pendingWaitingToSeat--;
                    TryPlaceWaitingPeople();
                    MaybeSettleIfIdle();
                });
            }
            break; // Process one color at a time for cleaner animation
        }

        if (!anyPlaced)
            MaybeSettleIfIdle();
    }

    private void MaybeSettleIfIdle()
    {
        if (pendingLaneTapBatches > 0 || pendingWaitingToSeat > 0) return;

        if (CurrentState == GameState.Animating)
            CurrentState = GameState.Playing;

        CheckWinLose();
    }

    /// <summary>Runs when a single person finishes sitting — filled visual + unlock along that seat's row/side chain (next neighbour inward).</summary>
    private void OnSomeoneSeated(object arg)
    {
        Seat seat = arg as Seat;
        if (seat != null && !seat.IsBlocked)
        {
            seat.RefreshFilledSeatVisual(
                colorThemeConfig != null ? colorThemeConfig.GetInaccessibleSeatMaterial(seat.Color) : null);
        }

        if (seat != null && !seat.IsBlocked)
            RefreshVacantAccessibilityForRowSide(seat.Row, seat.Side);
        else
            RefreshVacantSeatAccessibilityVisuals();

        TryPlaceWaitingPeople();
        MaybeSettleIfIdle();
    }

    /// <summary>
    /// Seats that look "unlocked" — same as logical accessibility from <see cref="SeatAccessibilityResolver"/> (outermost playable per row-side, per color).
    /// <see cref="Seat.SetAccessibilityVisual"/> skips unchanged state so only newly unlocked seats animate when the grid refreshes.
    /// </summary>
    private HashSet<Seat> BuildVisualAccessibleSeatSet()
    {
        var accessible = new HashSet<Seat>();

        foreach (SeatColor color in System.Enum.GetValues(typeof(SeatColor)))
        {
            List<Seat> seats = SeatAccessibilityResolver.FindAccessibleSeats(gridManager, color, int.MaxValue);
            for (int i = 0; i < seats.Count; i++)
                accessible.Add(seats[i]);
        }

        return accessible;
    }

    /// <summary>Updates vacant-seat visuals only on one row/side — the strip that contains the seat that was just filled, so the next playable neighbour animates without touching other rows.</summary>
    private void RefreshVacantAccessibilityForRowSide(int row, GridSide side)
    {
        if (row < 0 || row >= gridManager.RowCount) return;

        HashSet<Seat> accessible = BuildVisualAccessibleSeatSet();

        for (int d = 0; d < gridManager.SeatsPerSide; d++)
        {
            Seat s = gridManager.GetSeat(row, side, d);
            ApplyVacantAccessibilityVisual(s, accessible);
        }
    }

    private void RefreshVacantSeatAccessibilityVisuals()
    {
        HashSet<Seat> accessible = BuildVisualAccessibleSeatSet();

        for (int row = 0; row < gridManager.RowCount; row++)
        {
            for (int d = 0; d < gridManager.SeatsPerSide; d++)
            {
                ApplyVacantAccessibilityVisual(gridManager.GetSeat(row, GridSide.Left, d), accessible);
                ApplyVacantAccessibilityVisual(gridManager.GetSeat(row, GridSide.Right, d), accessible);
            }
        }
    }

    private void RefreshSeatAccessibilityVisuals()
    {
        HashSet<Seat> accessible = BuildVisualAccessibleSeatSet();

        for (int row = 0; row < gridManager.RowCount; row++)
        {
            for (int d = 0; d < gridManager.SeatsPerSide; d++)
            {
                RefreshSingleSeatVisual(gridManager.GetSeat(row, GridSide.Left, d), accessible);
                RefreshSingleSeatVisual(gridManager.GetSeat(row, GridSide.Right, d), accessible);
            }
        }
    }

    private void RefreshSingleSeatVisual(Seat seat, HashSet<Seat> accessible)
    {
        if (seat == null || seat.IsBlocked) return;
        if (seat.IsOccupied)
        {
            seat.RefreshFilledSeatVisual(
                colorThemeConfig != null ? colorThemeConfig.GetInaccessibleSeatMaterial(seat.Color) : null);
            return;
        }

        ApplyVacantAccessibilityVisual(seat, accessible);
    }

    private void ApplyVacantAccessibilityVisual(Seat seat, HashSet<Seat> accessible)
    {
        if (seat == null || seat.IsBlocked || seat.IsOccupied) return;

        // Resolver can mark an inner seat "reachable" while someone is still walking to an outer one
        // (it skips HasWalkIncoming and continues). Visually only unlock when the wall-side chain is filled,
        // or this seat is the active walk target.
        bool inLogicalPlaySet = accessible.Contains(seat) || seat.HasWalkIncoming;
        bool chainReady = seat.HasWalkIncoming || IsOutwardSeatChainFilled(seat);
        bool isAccessible = inLogicalPlaySet && chainReady;

        seat.SetAccessibilityVisual(
            isAccessible,
            colorThemeConfig != null ? colorThemeConfig.GetAccessibleSeatMaterial(seat.Color) : null,
            colorThemeConfig != null ? colorThemeConfig.GetInaccessibleSeatMaterial(seat.Color) : null
        );
    }

    /// <summary>Every seat closer to the far wall (lower aisle index) on this row/side is occupied.</summary>
    private bool IsOutwardSeatChainFilled(Seat seat)
    {
        if (seat == null || gridManager == null) return true;

        int row = seat.Row;
        GridSide side = seat.Side;
        for (int d = 0; d < seat.AisleDistance; d++)
        {
            Seat s = gridManager.GetSeat(row, side, d);
            if (s == null || s.IsBlocked) continue;
            if (!s.IsOccupied) return false;
        }

        return true;
    }

    private void CheckWinLose()
    {
        // Win: all lanes empty and no one waiting
        if (laneManager.AllLanesEmpty() && waitingAreaManager.CurrentCount == 0)
        {
            CurrentState = GameState.Won;
            EventController.TriggerEvent(GameEvent.EVENT_LEVEL_WIN);
            HapticManager.Instance?.PlayHaptics(HapticsID.HAPTIC_SUCCESS);

            // Trigger framework level end (win)
            LevelManager.Instance?.NextLevel(null);
            EventController.TriggerEvent(GameEvent.EVENT_LEVEL_ENDED, true);
            return;
        }

        // Lose: waiting area full and no valid moves
        if (waitingAreaManager.IsFull() && !HasAnyValidMove())
        {
            CurrentState = GameState.Lost;
            EventController.TriggerEvent(GameEvent.EVENT_LEVEL_LOSE);
            HapticManager.Instance?.PlayHaptics(HapticsID.HAPTIC_FAILURE);

            EventController.TriggerEvent(GameEvent.EVENT_LEVEL_ENDED, false);
            return;
        }
    }

    private bool HasAnyValidMove()
    {
        // If waiting area isn't full, any tap is valid (overflow goes to waiting)
        if (!waitingAreaManager.IsFull()) return true;

        // Check if any top lane group has matching accessible seats
        for (int i = 0; i < laneManager.LaneCount; i++)
        {
            PeopleGroup topGroup = laneManager.GetTopGroup(i);
            if (topGroup == null) continue;

            if (SeatAccessibilityResolver.HasAnyAccessibleSeat(gridManager, topGroup.Color))
                return true;
        }

        return false;
    }
}
