using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Accounts
{
    /// <summary>
    /// View model for user registration.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's NIF (tax identification number).
        /// </summary>
        [Required]
        [MaxLength(9)]
        [Display(Name = "NIF")]
        public string NIF { get; set; }

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        [Display(Name = "Role")]
        [Required]
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the uploaded profile image file.
        /// </summary>
        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        /// <summary>
        /// Gets or sets the URL of the profile image.
        /// </summary>
        public string? ProfileImageUrl { get; set; }
    }
}
