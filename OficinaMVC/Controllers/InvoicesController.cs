using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Services;
using Stripe.Checkout;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for managing invoices, including creation, payment, and email delivery.
    /// </summary>
    [Authorize(Roles = "Receptionist,Mechanic")]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceService _invoiceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoicesController"/> class.
        /// </summary>
        /// <param name="invoiceRepository">Repository for invoice data access.</param>
        /// <param name="invoiceService">Service for invoice-related business logic.</param>
        public InvoicesController(IInvoiceRepository invoiceRepository, IInvoiceService invoiceService)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Displays a list of invoices, optionally showing all or only unpaid ones.
        /// </summary>
        /// <param name="showAll">Whether to show all invoices or only unpaid ones.</param>
        /// <returns>The invoices index view.</returns>
        public async Task<IActionResult> Index(bool showAll = false)
        {
            var invoices = await _invoiceRepository.GetAllWithDetailsAsync(showAll);

            ViewData["ShowAll"] = showAll;

            return View(invoices);
        }

        /// <summary>
        /// Generates an invoice from a repair and redirects to its details.
        /// </summary>
        /// <param name="repairId">The repair ID to generate an invoice for.</param>
        /// <returns>Redirects to the invoice details or repairs index on error.</returns>
        public async Task<IActionResult> GenerateFromRepair(int repairId)
        {
            if (repairId == 0) return BadRequest();

            try
            {
                var invoice = await _invoiceService.GenerateForRepairAsync(repairId);
                return RedirectToAction("Details", new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Repairs");
            }
        }

        /// <summary>
        /// Displays the details of a specific invoice.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <returns>The invoice details view or not found.</returns>
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _invoiceService.GetInvoiceDetailViewModelAsync(id);
            if (viewModel == null) return NotFound();

            return View(viewModel);
        }

        /// <summary>
        /// Sends the invoice PDF to the client's email address.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <returns>Redirects to the invoices index with a success or error message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendByEmail(int id)
        {
            var response = await _invoiceService.SendInvoiceByEmailAsync(id);

            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Invoice PDF has been sent to the client's email successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response.Message ?? "There was an error sending the invoice email.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Creates a Stripe checkout session for invoice payment.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <returns>Redirects to the Stripe checkout page or not found.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(int id)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
        {
            "card"
        },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(invoice.TotalAmount * 100),
                    Currency = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"Invoice #{invoice.Id:D5} for Repair on {invoice.Repair.Vehicle.LicensePlate}"
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = $"{domain}/Invoices/PaymentSuccess?sessionId={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Invoices/PaymentCancel?invoiceId={invoice.Id}",
                Metadata = new Dictionary<string, string>
        {
            { "InvoiceId", invoice.Id.ToString() }
        }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        /// <summary>
        /// Handles successful payment and updates the invoice status.
        /// </summary>
        /// <param name="sessionId">The Stripe session ID.</param>
        /// <returns>The payment success view.</returns>
        public async Task<IActionResult> PaymentSuccess(string sessionId)
        {
            var sessionService = new SessionService();
            var session = sessionService.Get(sessionId);

            // Retrieve the invoice ID from the metadata
            var invoiceId = int.Parse(session.Metadata["InvoiceId"]);
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);

            if (invoice != null)
            {
                invoice.Status = "Paid";
                await _invoiceRepository.UpdateInvoiceAsync(invoice);
            }

            return View();
        }

        /// <summary>
        /// Handles payment cancellation and redirects to the invoice details.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <returns>Redirects to the invoice details view with an error message.</returns>
        public IActionResult PaymentCancel(int invoiceId)
        {
            TempData["ErrorMessage"] = "Payment was cancelled. You can try again at any time.";
            return RedirectToAction("Details", new { id = invoiceId });
        }
    }
}