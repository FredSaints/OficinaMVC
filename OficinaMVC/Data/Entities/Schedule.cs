using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a single work schedule block for a user, typically a mechanic,
    /// defining their availability on a specific day.
    /// </summary>
    public class Schedule : IEntity
    {
        /// <summary>
        /// The unique identifier for the schedule entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="User"/> this schedule belongs to.
        /// </summary>
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="User"/> (mechanic).
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The day of the week for this work schedule block.
        /// </summary>
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// The start time of the work block on the specified day.
        /// </summary>
        [Required]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The end time of the work block on the specified day.
        /// </summary>
        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
