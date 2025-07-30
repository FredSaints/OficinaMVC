using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Vehicles
{
    /// <summary>
    /// ViewModel representing a vehicle for create/edit forms.
    /// </summary>
    public class VehicleViewModel
    {
        /// <summary>
        /// Gets or sets the vehicle identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the vehicle.
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the year of the vehicle.
        /// </summary>
        [Required]
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the mileage of the vehicle in kilometers.
        /// </summary>
        [Required]
        [Display(Name = "Mileage (km)")]
        [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
        public int Mileage { get; set; }

        /// <summary>
        /// Gets or sets the fuel type of the vehicle.
        /// </summary>
        [Required]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        /// <summary>
        /// Gets or sets the brand identifier.
        /// </summary>
        [Required(ErrorMessage = "Please select a brand.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a brand.")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        /// <summary>
        /// Gets or sets the car model identifier.
        /// </summary>
        [Required(ErrorMessage = "Please select a model.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a model.")]
        [Display(Name = "Model")]
        public int CarModelId { get; set; }

        /// <summary>
        /// Gets or sets the list of available brands for selection.
        /// </summary>
        public IEnumerable<SelectListItem> Brands { get; set; }

        /// <summary>
        /// Gets or sets the list of available car models for selection.
        /// </summary>
        public IEnumerable<SelectListItem> CarModels { get; set; }

        /// <summary>
        /// Gets or sets the owner (client) identifier.
        /// </summary>
        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the list of available owners (clients) for selection.
        /// </summary>
        public IEnumerable<SelectListItem> OwnerList { get; set; } = new List<SelectListItem>();
    }
}