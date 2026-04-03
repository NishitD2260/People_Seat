using UnityEngine;

namespace PeopleSeat.Gameplay
{
    [DisallowMultipleComponent]
    public class LaneGroupInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameFlowController flow;
        [SerializeField] private int laneIndex;
        [SerializeField] private bool isFrontGroup;

        public int LaneIndex => laneIndex;

        /// <summary>Disable collider or set false when this group is not the lane front.</summary>
        public bool IsFrontGroup
        {
            get => isFrontGroup;
            set => isFrontGroup = value;
        }

        private void OnValidate()
        {
            if (flow == null)
                flow = GetComponentInParent<GameFlowController>();
        }

        public void OnTap()
        {
            if (!isFrontGroup || flow == null) return;
            flow.TryTapLane(laneIndex);
        }

        public void OnHoldStart() { }

        public void OnHolding() { }

        public void OnHoldEnd() { }

        public void OnBeginDrag() { }

        public void OnDrag(Vector3 pos) { }

        public void OnEndDrag() { }
    }
}
