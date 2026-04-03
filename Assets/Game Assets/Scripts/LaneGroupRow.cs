using UnityEngine;

namespace PeopleSeat.Gameplay
{
    /// <summary>
    /// Keeps only the current front group tappable: colliders enabled on front only, after each resolved tap.
    /// Scene order must match level SO groups front-to-back.
    /// </summary>
    public class LaneGroupRow : MonoBehaviour
    {
        [SerializeField] private GameFlowController flow;
        [SerializeField] private int laneIndex;
        [SerializeField] private LaneGroupInteractable[] groupsFrontToBack;

        private int _groupsAtStart = -1;

        private void Awake()
        {
            if (flow == null)
                flow = GetComponentInParent<GameFlowController>();
        }

        private void Start()
        {
            CaptureStartIfNeeded();
            SyncInteractivityFromState();
        }

        private void OnEnable()
        {
            if (flow != null)
                flow.OnPlacementResolved += OnPlacementResolved;
        }

        private void OnDisable()
        {
            if (flow != null)
                flow.OnPlacementResolved -= OnPlacementResolved;
        }

        private void OnPlacementResolved(int lane, PlacementOutcome outcome, int movedToWaiting)
        {
            if (lane == laneIndex)
                SyncInteractivityFromState();
        }

        private void CaptureStartIfNeeded()
        {
            if (_groupsAtStart >= 0) return;
            if (flow?.Lanes == null || laneIndex < 0 || laneIndex >= flow.Lanes.Lanes.Count)
                return;
            _groupsAtStart = flow.Lanes.Lanes[laneIndex].Count;
        }

        public void SyncInteractivityFromState()
        {
            CaptureStartIfNeeded();
            if (flow?.Lanes == null || laneIndex < 0 || laneIndex >= flow.Lanes.Lanes.Count)
                return;

            if (groupsFrontToBack == null || groupsFrontToBack.Length == 0) return;

            var remaining = flow.Lanes.Lanes[laneIndex].Count;
            if (remaining == 0)
            {
                for (var i = 0; i < groupsFrontToBack.Length; i++)
                {
                    var g = groupsFrontToBack[i];
                    if (g == null) continue;
                    g.IsFrontGroup = false;
                    if (g.TryGetComponent<Collider>(out var col))
                        col.enabled = false;
                }

                return;
            }

            if (_groupsAtStart < 0) return;

            if (groupsFrontToBack.Length < _groupsAtStart)
                Debug.LogWarning($"{nameof(LaneGroupRow)} lane {laneIndex}: fewer interactables ({groupsFrontToBack.Length}) than level groups ({_groupsAtStart}).");

            var consumed = Mathf.Clamp(_groupsAtStart - remaining, 0, groupsFrontToBack.Length - 1);

            for (var i = 0; i < groupsFrontToBack.Length; i++)
            {
                var g = groupsFrontToBack[i];
                if (g == null) continue;

                var isFront = i == consumed;
                g.IsFrontGroup = isFront;
                if (g.TryGetComponent<Collider>(out var col))
                    col.enabled = isFront;
            }
        }
    }
}
