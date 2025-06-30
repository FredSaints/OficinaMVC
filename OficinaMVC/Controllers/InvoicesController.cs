using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Invoices;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Receptionist,Mechanic")]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IViewRendererService _viewRenderer;
        private readonly IPdfService _pdfService;
        private readonly IConfiguration _configuration;

        public InvoicesController(IInvoiceRepository invoiceRepository,
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

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceRepository.GetAllWithDetailsAsync();
            return View(invoices);
        }

        public async Task<IActionResult> GenerateFromRepair(int repairId)
        {
            if (repairId == 0) return BadRequest();

            try
            {
                var invoice = await _invoiceRepository.GenerateInvoiceForRepairAsync(repairId);
                return RedirectToAction("Details", new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Repairs");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice == null) return NotFound();

            var viewModel = new InvoiceDetailViewModel
            {
                Invoice = invoice,
                IsEmail = false,
                CompanyName = _configuration["CompanyInfo:Name"],
                CompanyAddress = _configuration["CompanyInfo:Address"],
                CompanyPhone = _configuration["CompanyInfo:PhoneNumber"],
                CompanyNIF = _configuration["CompanyInfo:NIF"]
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendByEmail(int id)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice == null) return NotFound();

            var viewModelForEmail = new InvoiceDetailViewModel
            {
                Invoice = invoice,
                IsEmail = true,
                CompanyName = _configuration["CompanyInfo:Name"],
                CompanyAddress = _configuration["CompanyInfo:Address"],
                CompanyPhone = _configuration["CompanyInfo:PhoneNumber"],
                CompanyNIF = _configuration["CompanyInfo:NIF"]
            };

            var htmlBody = await _viewRenderer.RenderToStringAsync("/Views/Invoices/Details.cshtml", viewModelForEmail);

            var response = _mailHelper.SendEmailWithAttachment(
                invoice.Repair.Vehicle.Owner.Email,
                $"Your Invoice #{invoice.Id:D5} from FredAuto",
                "Please find your invoice attached.",
                _pdfService.GeneratePdf(htmlBody, viewModelForEmail.CompanyName),
                $"Invoice-{invoice.Id:D5}.pdf"
            );

            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Invoice PDF has been sent to the client's email successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "There was an error sending the invoice email.";
            }

            return RedirectToAction("Index");
        }
    }
}