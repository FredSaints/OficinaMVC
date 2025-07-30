using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Invoices;

namespace OficinaMVC.Services
{
    /// <inheritdoc cref="IInvoiceService"/>
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IViewRendererService _viewRenderer;
        private readonly IPdfService _pdfService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceService"/> class.
        /// </summary>
        /// <param name="invoiceRepository">The invoice repository for data access.</param>
        /// <param name="mailHelper">The mail helper service for sending emails.</param>
        /// <param name="viewRenderer">The view renderer service for rendering invoice views.</param>
        /// <param name="pdfService">The PDF service for generating invoice PDFs.</param>
        /// <param name="configuration">The application configuration for company info.</param>
        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IMailHelper mailHelper,
            IViewRendererService viewRenderer,
            IPdfService pdfService,
            IConfiguration configuration)
        {
            _invoiceRepository = invoiceRepository;
            _mailHelper = mailHelper;
            _viewRenderer = viewRenderer;
            _pdfService = pdfService;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<Invoice> GenerateForRepairAsync(int repairId)
        {
            return await _invoiceRepository.GenerateInvoiceForRepairAsync(repairId);
        }

        /// <inheritdoc />
        public async Task<InvoiceDetailViewModel?> GetInvoiceDetailViewModelAsync(int invoiceId, bool isEmail = false)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
            if (invoice == null)
            {
                return null;
            }

            return new InvoiceDetailViewModel
            {
                Invoice = invoice,
                IsEmail = isEmail,
                CompanyName = _configuration["CompanyInfo:Name"],
                CompanyAddress = _configuration["CompanyInfo:Address"],
                CompanyPhone = _configuration["CompanyInfo:PhoneNumber"],
                CompanyNIF = _configuration["CompanyInfo:NIF"]
            };
        }

        /// <inheritdoc />
        public async Task<Response> SendInvoiceByEmailAsync(int invoiceId)
        {
            var viewModelForEmail = await GetInvoiceDetailViewModelAsync(invoiceId, isEmail: true);
            if (viewModelForEmail == null)
            {
                return new Response { IsSuccess = false, Message = "Invoice not found." };
            }

            var htmlBody = await _viewRenderer.RenderToStringAsync("/Views/Invoices/Details.cshtml", viewModelForEmail);
            var pdfBytes = _pdfService.GeneratePdf(htmlBody, viewModelForEmail.CompanyName);

            var response = _mailHelper.SendEmailWithAttachment(
                viewModelForEmail.Invoice.Repair.Vehicle.Owner.Email,
                $"Your Invoice #{viewModelForEmail.Invoice.Id:D5} from FredAuto",
                "Please find your invoice attached.",
                pdfBytes,
                $"Invoice-{viewModelForEmail.Invoice.Id:D5}.pdf"
            );

            return response;
        }
    }
}