using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Accounts
{
    /// <summary>
    /// View model for recovering a user's password.
    /// </summary>
    public class RecoverPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the user's email address for password recovery.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}