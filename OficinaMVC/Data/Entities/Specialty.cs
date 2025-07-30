﻿using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a specific skill or area of expertise for a mechanic (e.g., "Diesel Engines", "Transmissions").
    /// </summary>
    public class Specialty : IEntity
    {
        /// <summary>
        /// The unique identifier for the specialty.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the specialty.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Navigation property for the many-to-many relationship with <see cref="User"/>,
        /// linking mechanics to their specialties.
        /// </summary>
        public ICollection<UserSpecialty> UserSpecialties { get; set; } = new List<UserSpecialty>();
    }
}
