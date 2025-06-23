using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Vehicles
{
    public class CarModelViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Model Name")]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a brand.")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        //public IEnumerable<SelectListItem> Brands { get; set; }
    }
}
