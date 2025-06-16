using OficinaMVC.Models.Enums;

namespace OficinaMVC.Models.Vehicles
{
    public class VehicleListViewModel
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string CarModel { get; set; }
        public int Year { get; set; }
        public FuelType FuelType { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
    }
}
