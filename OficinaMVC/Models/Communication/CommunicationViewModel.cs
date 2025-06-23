using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Communication
{
    public class CommunicationViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Message Body")]
        [DataType(DataType.Html)]
        public string Message { get; set; }
    }
}
