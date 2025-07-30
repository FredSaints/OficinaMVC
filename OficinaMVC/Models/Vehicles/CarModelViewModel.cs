using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Vehicles
{
    /// <summary>
    /// ViewModel representing a car model for create/edit forms.
    /// </summary>
    public class CarModelViewModel
    {
        /// <summary>
        /// Gets or sets the car model identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the car model.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Model Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the brand identifier associated with the car model.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Please select a brand.")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        //public IEnumerable<SelectListItem> Brands { get; set; }
    }
}
