using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{

    public class Schedule : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
