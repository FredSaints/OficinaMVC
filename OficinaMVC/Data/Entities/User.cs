using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(9)]
        [Display(Name = "Fiscal Number")]
        public string NIF { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfileImageUrl { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        public ICollection<UserSpecialty>? UserSpecialties { get; set; }
        public ICollection<Schedule>? Schedules { get; set; }
    }
}