using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    public class Brand : IEntity
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public ICollection<CarModel> CarModels { get; set; } = new List<CarModel>();
    }
}
