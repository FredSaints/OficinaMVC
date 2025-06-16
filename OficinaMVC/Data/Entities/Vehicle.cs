using OficinaMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    public class Vehicle : IEntity
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
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

   
        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public User Owner { get; set; }


        public List<Repair> Repairs { get; set; } = new List<Repair>();
    }
}
