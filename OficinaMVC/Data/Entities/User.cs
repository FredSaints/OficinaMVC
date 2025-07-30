using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Extends the base IdentityUser to include application-specific properties for all users,
    /// such as clients, mechanics, and administrators.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// The user's first name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// The user's unique fiscal identification number (NIF).
        /// </summary>
        [Required]
        [MaxLength(9)]
        [Display(Name = "Fiscal Number")]
        public string NIF { get; set; }

        /// <summary>
        /// The URL for the user's profile picture.
        /// </summary>
        [Display(Name = "Profile Picture")]
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// A collection of vehicles owned by the user, if they are a client.
        /// </summary>
        public ICollection<Vehicle>? Vehicles { get; set; }
        /// <summary>
        /// A collection of appointments associated with the user, either as a client or a mechanic.
        /// </summary>
        public ICollection<Appointment>? Appointments { get; set; }

        /// <summary>
        /// A calculated property that returns the user's full name.
        /// </summary>
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Navigation property for the many-to-many relationship with <see cref="Specialty"/>.
        /// This links a mechanic to their areas of expertise.
        /// </summary>
        public ICollection<UserSpecialty> UserSpecialties { get; set; }//TODO: verificar se pode ser nullable
        /// <summary>
        /// A collection of work schedules for the user, if they are staff (e.g., a mechanic).
        /// </summary>
        public ICollection<Schedule>? Schedules { get; set; }
    }
}