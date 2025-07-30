using OficinaMVC.Data.Entities;

namespace OficinaMVC.Models.Home
{
    /// <summary>
    /// ViewModel representing the data for the home page.
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Gets or sets the list of available services (repair types).
        /// </summary>
        public List<RepairType> Services { get; set; }

        /// <summary>
        /// Gets or sets the list of public mechanics to display.
        /// </summary>
        public List<PublicMechanicViewModel> Mechanics { get; set; }

        /// <summary>
        /// Gets or sets the opening hours for each day of the week.
        /// </summary>
        public Dictionary<DayOfWeek, string> OpeningHours { get; set; }
    }

    /// <summary>
    /// ViewModel representing a public mechanic for the home page.
    /// </summary>
    public class PublicMechanicViewModel
    {
        /// <summary>
        /// Gets or sets the full name of the mechanic.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the profile image URL of the mechanic.
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of specialties for the mechanic.
        /// </summary>
        public List<string> Specialties { get; set; } = new List<string>();
    }
}