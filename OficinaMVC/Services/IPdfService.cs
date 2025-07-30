namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for generating PDF documents from HTML content.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Generates a PDF document from the provided HTML content and company name.
        /// </summary>
        /// <param name="htmlContent">The HTML content to convert to PDF.</param>
        /// <param name="companyName">The company name for document title/footer.</param>
        /// <returns>The generated PDF as a byte array.</returns>
        byte[] GeneratePdf(string htmlContent, string companyName);
    }
}