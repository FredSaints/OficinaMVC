using OficinaMVC.Data.Entities;

namespace OficinaMVC.Models.Mechanics
{
    /// <summary>
    /// ViewModel for editing mechanic details, specialties, and schedules.
    /// </summary>
    public class MechanicEditViewModel
    {
        /// <summary>
        /// Gets or sets the user identifier of the mechanic.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the mechanic.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the profile image URL of the mechanic (optional).
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of selected specialty IDs for the mechanic.
        /// </summary>
        public List<int> SelectedSpecialtyIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the list of available specialties.
        /// </summary>
        public List<Specialty> AvailableSpecialties { get; set; } = new List<Specialty>();

        /// <summary>
        /// Gets or sets the list of schedules for the mechanic.
        /// </summary>
        public List<ScheduleViewModel> Schedules { get; set; } = new List<ScheduleViewModel>();
    }
}
