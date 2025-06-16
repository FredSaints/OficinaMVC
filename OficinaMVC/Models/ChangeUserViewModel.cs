using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models
{
    public class ChangeUserViewModel
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }
        public string? CurrentProfileImageUrl { get; set; }
    }
}
