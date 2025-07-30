using DinkToPdf;
using DinkToPdf.Contracts;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Service for generating PDF documents from HTML content using DinkToPdf.
    /// </summary>
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _hostingEnvironment;
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfService"/> class.
        /// </summary>
        /// <param name="converter">The DinkToPdf converter instance.</param>
        /// <param name="hostingEnvironment">The web hosting environment.</param>
        public PdfService(IConverter converter, IWebHostEnvironment hostingEnvironment)
        {
            _converter = converter;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <inheritdoc />
        public byte[] GeneratePdf(string htmlContent, string companyName)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
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
                    Center = "Thank you for your business!",
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
            objectSettings.WebSettings.PrintMediaType = true;
            objectSettings.WebSettings.EnableJavascript = false;


            objectSettings.WebSettings.LoadImages = false;

            var pdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return _converter.Convert(pdfDocument);
        }
    }
}