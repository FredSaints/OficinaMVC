using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Accounts
{
    /// <summary>
    /// View model for changing user profile information.
    /// </summary>
    public class ChangeUserViewModel
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
        /// Gets or sets the user's phone number.
        /// </summary>
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the uploaded profile image file.
        /// </summary>
        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        /// <summary>
        /// Gets or sets the URL of the current profile image.
        /// </summary>
        public string? CurrentProfileImageUrl { get; set; }
    }
}
