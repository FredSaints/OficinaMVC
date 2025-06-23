using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    public class CarModel : IEntity
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
    }
}
