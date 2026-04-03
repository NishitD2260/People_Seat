namespace PeopleSeat.Gameplay
{
    /// <summary>
    /// Single-source design defaults for QA and implementation. Do not edit the plan file; tune here.
    /// </summary>
    public static class SeatingGameRules
    {
        /// <summary>Default max people in waiting area when not overridden by level.</summary>
        public const int DefaultWaitingAreaCapacity = 10;

        /// <summary>
        /// Waiting area full: lane tap that would add anyone to waiting is blocked (no move, no consume lane group).
        /// </summary>
        public const string WaitingFullPolicy =
            "Block tap when any overflow would exceed waiting capacity; do not dequeue lane group.";

        /// <summary>
        /// Win: every lane has no groups left and waiting area has zero people.
        /// </summary>
        public const string WinCondition = "All lane queues empty and waiting area empty.";

        /// <summary>
        /// Lose: waiting is full and the front group of at least one lane cannot place anyone (optional meta hook).
        /// </summary>
        public const string LoseConditionHint =
            "When waiting is at capacity and no seated/waiting resolution is possible — level fail or soft-lock per meta.";

        /// <summary>
        /// Among equal-aisleProgress seats, lower seat id wins for determinism.
        /// </summary>
        public const string SeatIdTieBreak = "Stable tie-break: sort by seat id after aisleProgress and depth.";

        /// <summary>
        /// Eligible seats: matching color, empty, accessible from aisle chain; sort by aisleProgress asc, then depth desc, then id.
        /// </summary>
        public const string AisleWalkEncounterOrder =
            "First matching accessible seat along ascending aisleProgress (center path); secondary: deeper-from-aisle before shallower.";

        /// <summary>
        /// Apply grid mutations immediately; play animations after so rapid taps cannot desync occupancy.
        /// </summary>
        public const string StateBeforeAnimation = "Commit GridState and waiting/lane counts before starting tweens.";
    }
}
