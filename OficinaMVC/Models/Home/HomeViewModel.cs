using OficinaMVC.Data.Entities;

namespace OficinaMVC.Models.Home
{
    public class HomeViewModel
    {
        public List<RepairType> Services { get; set; }

        public List<PublicMechanicViewModel> Mechanics { get; set; }

        public Dictionary<DayOfWeek, string> OpeningHours { get; set; }
    }

    public class PublicMechanicViewModel
    {
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<string> Specialties { get; set; } = new List<string>();
    }
}