namespace OficinaMVC.Models.Dashboard
{
    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
    }
}
