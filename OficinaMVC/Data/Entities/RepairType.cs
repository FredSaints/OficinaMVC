using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a predefined type of service or repair offered by the workshop (e.g., "Oil Change", "Brake Inspection").
    /// </summary>
    public class RepairType : IEntity
    {
        /// <summary>
        /// The unique identifier for the repair type.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the service.
        /// </summary>
        [Required, MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// An optional description of what the service includes.
        /// </summary>
        [MaxLength(250)]
        public string Description { get; set; }
    }
}
