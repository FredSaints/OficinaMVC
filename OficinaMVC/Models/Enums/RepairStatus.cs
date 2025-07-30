namespace OficinaMVC.Models.Enums
{
    /// <summary>
    /// Represents the status of a repair process.
    /// </summary>
    public enum RepairStatus
    {
        /// <summary>
        /// The repair is open and has not started yet.
        /// </summary>
        Open,

        /// <summary>
        /// The repair is waiting for parts to arrive.
        /// </summary>
        WaitingForParts,

        /// <summary>
        /// The repair is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The repair has been completed.
        /// </summary>
        Completed,

        /// <summary>
        /// The repair has been cancelled.
        /// </summary>
        Cancelled
    }
}
