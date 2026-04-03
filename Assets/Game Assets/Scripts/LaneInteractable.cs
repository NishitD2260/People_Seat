using UnityEngine;

namespace PeopleSeat.Gameplay
{
    /// <summary>Optional: one collider per lane when the whole front stack shares a tap zone.</summary>
    [DisallowMultipleComponent]
    public class LaneInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameFlowController flow;
        [SerializeField] private int laneIndex;

        private void Awake()
        {
            if (flow == null)
                flow = GetComponentInParent<GameFlowController>();
        }

        private void OnValidate()
        {
            if (flow == null)
                flow = GetComponentInParent<GameFlowController>();
        }

        public void OnTap() => flow?.TryTapLane(laneIndex);

        public void OnHoldStart() { }

        public void OnHolding() { }

        public void OnHoldEnd() { }

        public void OnBeginDrag() { }

        public void OnDrag(Vector3 pos) { }

        public void OnEndDrag() { }
    }
}
