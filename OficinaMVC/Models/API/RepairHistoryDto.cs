namespace OficinaMVC.Models.API
{
    public class RepairHistoryDto
    {
        public int RepairId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal TotalCost { get; set; }
        public List<PartUsedDto> PartsUsed { get; set; } = new List<PartUsedDto>();
        public List<string> Mechanics { get; set; }
    }

    public class PartUsedDto
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}