using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    public class Specialty : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        // Navigation property for many-to-many
        public ICollection<UserSpecialty> UserSpecialties { get; set; } = new List<UserSpecialty>();
    }
}
