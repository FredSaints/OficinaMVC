using System.ComponentModel.DataAnnotations;
namespace OficinaMVC.Models.Accounts;
/// <summary>
/// View model for resetting a user's password.
/// </summary>
public class ResetPasswordViewModel
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the confirmation for the new password.
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// Gets or sets the password reset token.
    /// </summary>
    [Required]
    public string Token { get; set; }
}