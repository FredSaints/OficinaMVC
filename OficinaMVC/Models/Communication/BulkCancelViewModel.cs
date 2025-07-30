using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Communication
{
    /// <summary>
    /// View model for bulk cancellation of appointments for a selected mechanic.
    /// </summary>
    public class BulkCancelViewModel
    {
        /// <summary>
        /// Gets or sets the selected mechanic's ID for which appointments will be cancelled.
        /// </summary>
        [Required(ErrorMessage = "You must select a mechanic.")]
        [Display(Name = "Select Mechanic to Cancel Appointments For")]
        public string MechanicId { get; set; }

        /// <summary>
        /// Gets or sets the list of available mechanics.
        /// </summary>
        public IEnumerable<SelectListItem> Mechanics { get; set; }
    }
}
