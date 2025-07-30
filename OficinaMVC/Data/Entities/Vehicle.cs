using OficinaMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a client's vehicle registered in the system.
    /// </summary>
    public class Vehicle : IEntity
    {
        /// <summary>
        /// The unique identifier for the vehicle.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The vehicle's unique license plate number.
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="CarModel"/> of the vehicle.
        /// </summary>
        [Required]
        [Display(Name = "Model")]
        public int CarModelId { get; set; }

        /// <summary>
        /// Navigation property to the <see cref="CarModel"/>, which includes brand information.
        /// </summary>
        [ForeignKey("CarModelId")]
        public CarModel CarModel { get; set; }

        /// <summary>
        /// The manufacturing year of the vehicle.
        /// </summary>
        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        /// <summary>
        /// The current mileage of the vehicle in kilometers.
        /// </summary>
        [Required]
        [Display(Name = "Mileage (km)")]
        [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
        public int Mileage { get; set; }

        /// <summary>
        /// The type of fuel the vehicle uses (e.g., Gasoline, Diesel, Electric).
        /// </summary>
        [Required]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="User"/> who owns the vehicle.
        /// </summary>
        [Required]
        public string OwnerId { get; set; }

        /// <summary>
        /// Navigation property to the owner <see cref="User"/>.
        /// </summary>
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        /// <summary>
        /// A collection of all repair jobs performed on this vehicle.
        /// </summary>
        public List<Repair> Repairs { get; set; } = new List<Repair>();
    }
}