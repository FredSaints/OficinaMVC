
using OficinaMVC.Models.Enums;

namespace OficinaMVC.Models.Vehicles
{
    /// <summary>
    /// ViewModel representing a vehicle for list views.
    /// </summary>
    public class VehicleListViewModel
    {
        /// <summary>
        /// Gets or sets the vehicle identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the vehicle.
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the brand name of the vehicle.
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Gets or sets the car model name.
        /// </summary>
        public string CarModel { get; set; }

        /// <summary>
        /// Gets or sets the year of the vehicle.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the mileage of the vehicle.
        /// </summary>
        public int Mileage { get; set; }

        /// <summary>
        /// Gets or sets the fuel type of the vehicle.
        /// </summary>
        public FuelType FuelType { get; set; }

        /// <summary>
        /// Gets or sets the owner's name.
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets the owner's email address.
        /// </summary>
        public string OwnerEmail { get; set; }
    }
}