using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models
{
    public class RegisterViewModel
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
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [MaxLength(9)]
        [Display(Name = "NIF")]
        public string NIF { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Role")]
        [Required]
        public string Role { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
