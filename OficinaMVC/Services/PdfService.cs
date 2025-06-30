using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting; // Still useful if you ever re-enable images and need BaseUrl
// using System.IO; // Only needed if you were constructing paths for UserStyleSheet or BaseUrl

namespace OficinaMVC.Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _hostingEnvironment; // Keep for potential future use

        public PdfService(IConverter converter, IWebHostEnvironment hostingEnvironment)
        {
            _converter = converter;
            _hostingEnvironment = hostingEnvironment; // Store it even if not used immediately
        }

        // companyName parameter is still here as per IPdfService, useful for DocumentTitle/Footer
        public byte[] GeneratePdf(string htmlContent, string companyName)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color, // or ColorMode.Grayscale if you prefer B&W PDFs
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 15, Bottom = 15, Left = 10, Right = 10 },
                DocumentTitle = $"Invoice - {companyName}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                HeaderSettings = {
                    FontName = "Arial",
                    FontSize = 9,
                    Right = "Page [page] of [toPage]",
                    Line = true,
                    Spacing = 3
                },
                FooterSettings = {
                    FontName = "Arial",
                    FontSize = 9,
                    Left = $"© {DateTime.Now.Year} {companyName}",
                    Center = "", // Or "Thank you for your business!" if you want it in the PDF footer
                    Right = "[date]",
                    Line = true,
                    Spacing = 3
                }
            };

            // Initialize WebSettings if it's null (it usually isn't but good practice)
            if (objectSettings.WebSettings == null)
            {
                objectSettings.WebSettings = new WebSettings();
            }

            // Configure WebSettings for PDF generation
            objectSettings.WebSettings.DefaultEncoding = "utf-8";
            objectSettings.WebSettings.PrintMediaType = true;   // Use CSS @media print rules
            objectSettings.WebSettings.EnableJavascript = false;  // Typically false for invoices

            // --- KEY CHANGE: Disable image loading for the PDF ---
            objectSettings.WebSettings.LoadImages = false;

            // 'EnableLocalFileAccess' and 'BaseUrl' are less critical if images are disabled.
            // However, 'PrintMediaType = true' is still important for applying your print CSS.

            // If you had CustomFlags for other reasons, they would go here.
            // For now, with images disabled, we don't need to force --enable-local-file-access
            // if (objectSettings.WebSettings.CustomFlags == null)
            // {
            //     objectSettings.WebSettings.CustomFlags = new Dictionary<string, string?>();
            // }
            // Example:
            // if (!objectSettings.WebSettings.CustomFlags.ContainsKey("--disable-smart-shrinking"))
            // {
            //     objectSettings.WebSettings.CustomFlags.Add("--disable-smart-shrinking", null);
            // }

            var pdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return _converter.Convert(pdfDocument);
        }
    }
}