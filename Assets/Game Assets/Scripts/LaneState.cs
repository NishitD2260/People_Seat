using System;
using System.Collections.Generic;

namespace PeopleSeat.Gameplay
{
    [Serializable]
    public struct LaneGroup
    {
        public PersonColor Color;
        public int Count;
    }

    /// <summary>Ordered queue of groups per lane; index 0 is the tap front.</summary>
    public sealed class LaneState
    {
        public List<List<LaneGroup>> Lanes { get; }

        public LaneState(IReadOnlyList<LaneDefinitionAuthoring> laneDefinitions)
        {
            Lanes = new List<List<LaneGroup>>();
            if (laneDefinitions == null) return;

            foreach (var def in laneDefinitions)
            {
                var q = new List<LaneGroup>();
                if (def?.GroupsFrontToBack != null)
                {
                    foreach (var g in def.GroupsFrontToBack)
                        q.Add(new LaneGroup { Color = g.Color, Count = g.Count });
                }

                Lanes.Add(q);
            }
        }

        public bool HasGroup(int laneIndex) =>
            laneIndex >= 0 && laneIndex < Lanes.Count && Lanes[laneIndex].Count > 0;

        public LaneGroup? PeekFront(int laneIndex)
        {
            if (!HasGroup(laneIndex)) return null;
            return Lanes[laneIndex][0];
        }

        public void DequeueFront(int laneIndex)
        {
            if (!HasGroup(laneIndex)) return;
            Lanes[laneIndex].RemoveAt(0);
        }

        public bool AllLanesEmpty()
        {
            foreach (var lane in Lanes)
            {
                if (lane.Count > 0) return false;
            }

            return true;
        }
    }
}
