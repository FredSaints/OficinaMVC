using OficinaMVC.Data.Entities;

namespace OficinaMVC.Models.Mechanics
{
    public class MechanicEditViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string? ProfileImageUrl { get; set; }

        public List<int> SelectedSpecialtyIds { get; set; } = new List<int>();
        public List<Specialty> AvailableSpecialties { get; set; } = new List<Specialty>();
        public List<ScheduleViewModel> Schedules { get; set; } = new List<ScheduleViewModel>();
    }
}
