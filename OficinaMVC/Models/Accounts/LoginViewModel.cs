using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Accounts
{
    /// <summary>
    /// View model for user login.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the user's email address (used as username).
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remember the user on this device.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
