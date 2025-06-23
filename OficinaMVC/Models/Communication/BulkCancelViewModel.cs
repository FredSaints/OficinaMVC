using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Communication
{
    public class BulkCancelViewModel
    {
        [Required(ErrorMessage = "You must select a mechanic.")]
        [Display(Name = "Select Mechanic to Cancel Appointments For")]
        public string MechanicId { get; set; }

        public IEnumerable<SelectListItem> Mechanics { get; set; }
    }
}
