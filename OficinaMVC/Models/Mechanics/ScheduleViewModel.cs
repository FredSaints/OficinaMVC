namespace OficinaMVC.Models.Mechanics
{
    /// <summary>
    /// ViewModel representing a mechanic's work schedule for a specific day.
    /// </summary>
    public class ScheduleViewModel
    {
        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the day of the week for the schedule.
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the start time of the schedule.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the schedule.
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
