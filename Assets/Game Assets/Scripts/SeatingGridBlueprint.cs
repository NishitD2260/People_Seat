using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeopleSeat.Gameplay
{
    /// <summary>
    /// Optional author-time grid (four quadrants). Row 0 in bottom quadrants is the row nearest the waiting/lanes;
    /// row 0 in top quadrants is the row nearest the horizontal divider. Left blocks: col 0 is farthest from the aisle.
    /// Right blocks: col 0 is aisle-adjacent.
    /// </summary>
    [Serializable]
    public class SeatingGridBlueprint
    {
        [Min(0)] public int RowsBottom = 5;
        [Min(0)] public int RowsTop = 4;
        [Min(0)] public int ColsLeft = 3;
        [Min(0)] public int ColsRight = 4;

        public List<PersonColor> BottomLeft = new List<PersonColor>();
        public List<PersonColor> TopLeft = new List<PersonColor>();
        public List<PersonColor> BottomRight = new List<PersonColor>();
        public List<PersonColor> TopRight = new List<PersonColor>();

        public void EnsureListSizes(PersonColor fill = PersonColor.Red)
        {
            Resize(BottomLeft, RowsBottom * ColsLeft, fill);
            Resize(TopLeft, RowsTop * ColsLeft, fill);
            Resize(BottomRight, RowsBottom * ColsRight, fill);
            Resize(TopRight, RowsTop * ColsRight, fill);
        }

        private static void Resize(List<PersonColor> list, int n, PersonColor fill)
        {
            if (n < 0) n = 0;
            while (list.Count < n)
                list.Add(fill);
            if (list.Count > n)
                list.RemoveRange(n, list.Count - n);
        }
    }
}
