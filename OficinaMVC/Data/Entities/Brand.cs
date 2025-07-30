using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a vehicle manufacturer brand (e.g., Ford, Toyota, BMW).
    /// </summary>
    public class Brand : IEntity
    {
        /// <summary>
        /// The unique identifier for the brand.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the brand.
        /// </summary>
        [Required, MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// A collection of car models associated with this brand.
        /// </summary>
        public ICollection<CarModel> CarModels { get; set; } = new List<CarModel>();
    }
}