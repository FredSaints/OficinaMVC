using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Data.Entities
{
    public class RepairType : IEntity
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        // TODO: public decimal DefaultPrice { get; set; } - se quiser adicionar no futuro
    }
}
