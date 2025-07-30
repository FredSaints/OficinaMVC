using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Communication
{
    /// <summary>
    /// View model for sending a communication message.
    /// </summary>
    public class CommunicationViewModel
    {
        /// <summary>
        /// Gets or sets the subject of the message.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        [Required]
        [Display(Name = "Message Body")]
        [DataType(DataType.Html)]
        public string Message { get; set; }
    }
}
