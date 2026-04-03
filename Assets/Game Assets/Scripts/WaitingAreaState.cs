using System.Collections.Generic;

namespace PeopleSeat.Gameplay
{
    public sealed class WaitingAreaState
    {
        public int Capacity { get; }
        public IReadOnlyList<PersonColor> People => _people;
        public int Count => _people.Count;

        private readonly List<PersonColor> _people;

        public WaitingAreaState(int capacity)
        {
            Capacity = capacity;
            _people = new List<PersonColor>(capacity);
        }

        public bool CanAccept(int additional) => Count + additional <= Capacity;

        public void EnqueueMany(IReadOnlyList<PersonColor> colors)
        {
            for (var i = 0; i < colors.Count; i++)
                _people.Add(colors[i]);
        }

        public void EnqueueManySame(PersonColor color, int n)
        {
            for (var i = 0; i < n; i++)
                _people.Add(color);
        }

        /// <summary>Remove one person from front (FIFO).</summary>
        public bool TryDequeueFront(out PersonColor color)
        {
            if (_people.Count == 0)
            {
                color = default;
                return false;
            }

            color = _people[0];
            _people.RemoveAt(0);
            return true;
        }

        public void Clear() => _people.Clear();

        public bool TryPeekFront(out PersonColor color)
        {
            if (_people.Count == 0)
            {
                color = default;
                return false;
            }

            color = _people[0];
            return true;
        }
    }
}
