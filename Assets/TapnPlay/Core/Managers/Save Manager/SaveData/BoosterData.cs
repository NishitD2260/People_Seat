using System;
using System.Collections.Generic;

namespace TapNPlay.Core.Data
{
    [Serializable]
    public class BoosterData
    {
        public List<BoosterCount> BoosterCounts;

        public BoosterData()
        {
            BoosterCounts = new List<BoosterCount>();
            foreach (Booster B in Enum.GetValues(typeof(Booster)))
            {
                BoosterCounts.Add(new BoosterCount(B, 1));
            }
        }

        public BoosterCount GetBoosterCount(Booster boosterType)
        {
            return BoosterCounts.Find(bc => bc.BoosterType == boosterType);
        }
    }

    [Serializable]
    public struct BoosterCount
    {
        public Booster BoosterType;
        public int Count;

        public BoosterCount(Booster boosterType, int count)
        {
            BoosterType = boosterType;
            Count = count;
        }
    }
}