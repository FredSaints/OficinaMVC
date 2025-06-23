using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Vehicles
{
    public class VehicleViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        [Required]
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Mileage (km)")]
        [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
        public int Mileage { get; set; }

        [Required]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        [Required(ErrorMessage = "Please select a brand.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a brand.")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Please select a model.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a model.")]
        [Display(Name = "Model")]
        public int CarModelId { get; set; }

        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<SelectListItem> CarModels { get; set; }

        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public string OwnerId { get; set; }

        public IEnumerable<SelectListItem> OwnerList { get; set; } = new List<SelectListItem>();
    }
}