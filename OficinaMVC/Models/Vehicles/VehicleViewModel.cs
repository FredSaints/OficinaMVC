using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        [MaxLength(30)]
        public string Brand { get; set; }

        [Required]
        [MaxLength(30)]
        public string CarModel { get; set; }

        [Required]
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        [Required(ErrorMessage = "Please select a client")]
        [Display(Name = "Client")]
        public string OwnerId { get; set; }

        [BindNever]
        public List<SelectListItem> OwnerList { get; set; }
    }
}