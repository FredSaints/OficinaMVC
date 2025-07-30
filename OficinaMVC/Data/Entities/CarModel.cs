using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a specific model of a vehicle (e.g., Focus, Corolla, X5).
    /// </summary>
    public class CarModel : IEntity
    {
        /// <summary>
        /// The unique identifier for the car model.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the car model.
        /// </summary>
        [Required, MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Brand"/> that this model belongs to.
        /// </summary>
        [Required]
        public int BrandId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="Brand"/>.
        /// </summary>
        public Brand Brand { get; set; }
    }
}