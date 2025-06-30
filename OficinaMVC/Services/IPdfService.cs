namespace OficinaMVC.Services
{
    public interface IPdfService
    {
        // Modified to accept companyName
        byte[] GeneratePdf(string htmlContent, string companyName);
    }
}