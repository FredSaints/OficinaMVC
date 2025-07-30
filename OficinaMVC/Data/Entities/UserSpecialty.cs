namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents the join entity for the many-to-many relationship between a <see cref="User"/> (mechanic)
    /// and a <see cref="Specialty"/>.
    /// </summary>
    public class UserSpecialty
    {
        /// <summary>
        /// The foreign key for the <see cref="User"/>.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="User"/>.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Specialty"/>.
        /// </summary>
        public int SpecialtyId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="Specialty"/>.
        /// </summary>
        public Specialty Specialty { get; set; }
    }
}
