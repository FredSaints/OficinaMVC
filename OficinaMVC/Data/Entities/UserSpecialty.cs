namespace OficinaMVC.Data.Entities
{
    public class UserSpecialty
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int SpecialtyId { get; set; }
        public Specialty Specialty { get; set; }
    }
}
