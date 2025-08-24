using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.InvoiceCRUD.DTOs;
using YopoBackend.Modules.InvoiceCRUD.Services;
using YopoBackend.Services;
using System.Security.Claims;
using YopoBackend.Attributes;

namespace YopoBackend.Modules.InvoiceCRUD.Controllers
{
    /// <summary>
    /// Controller for managing invoice operations.
    /// Module: InvoiceCRUD (Module ID: 7)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(7)] // InvoiceCRUD module
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the InvoicesController class.
        /// </summary>
        /// <param name="invoiceService">The invoice service.</param>
        /// <param name="jwtService">The JWT service for user authentication.</param>
        public InvoicesController(IInvoiceService invoiceService, IJwtService jwtService)
        {
            _invoiceService = invoiceService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Gets all invoices with pagination and filtering support.
        /// </summary>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <param name="customerId">Optional customer ID to filter by.</param>
        /// <param name="buildingId">Optional building ID to filter by.</param>
        /// <param name="month">Optional month to filter by.</param>
        /// <param name="year">Optional year to filter by.</param>
        /// <param name="status">Optional status to filter by.</param>
        /// <param name="paidOnly">Whether to include only paid invoices.</param>
        /// <param name="unpaidOnly">Whether to include only unpaid invoices.</param>
        /// <returns>A paginated list of invoices.</returns>
        [HttpGet]
        public async Task<ActionResult<InvoiceListResponseDTO>> GetAllInvoices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? customerId = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null,
            [FromQuery] string? status = null,
            [FromQuery] bool paidOnly = false,
            [FromQuery] bool unpaidOnly = false)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _invoiceService.GetAllInvoicesAsync(
                    page, pageSize, customerId, buildingId, month, year, status, paidOnly, unpaidOnly);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving invoices.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <returns>The invoice if found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceResponseDTO>> GetInvoiceById(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { message = $"Invoice with ID {id} not found." });
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the invoice.", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        /// <param name="createInvoiceDto">The invoice creation data.</param>
        /// <returns>The created invoice.</returns>
        [HttpPost]
        public async Task<ActionResult<InvoiceResponseDTO>> CreateInvoice([FromBody] CreateInvoiceDTO createInvoiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var invoice = await _invoiceService.CreateInvoiceAsync(createInvoiceDto, currentUserId);

                return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.InvoiceId }, invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the invoice.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing invoice.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <param name="updateInvoiceDto">The invoice update data.</param>
        /// <returns>The updated invoice.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<InvoiceResponseDTO>> UpdateInvoice(int id, [FromBody] UpdateInvoiceDTO updateInvoiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var invoice = await _invoiceService.UpdateInvoiceAsync(id, updateInvoiceDto);
                if (invoice == null)
                {
                    return NotFound(new { message = $"Invoice with ID {id} not found." });
                }

                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the invoice.", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an invoice by ID.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInvoice(int id)
        {
            try
            {
                var success = await _invoiceService.DeleteInvoiceAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Invoice with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the invoice.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all invoices for a specific customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A paginated list of invoices for the customer.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<InvoiceListResponseDTO>> GetInvoicesByCustomerId(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _invoiceService.GetInvoicesByCustomerIdAsync(customerId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving invoices for the customer.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all invoices for a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A paginated list of invoices for the building.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<InvoiceListResponseDTO>> GetInvoicesByBuildingId(
            int buildingId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _invoiceService.GetInvoicesByBuildingIdAsync(buildingId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving invoices for the building.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets invoices for a specific period (month and year).
        /// </summary>
        /// <param name="month">The month (1-12).</param>
        /// <param name="year">The year.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A paginated list of invoices for the period.</returns>
        [HttpGet("period/{month}/{year}")]
        public async Task<ActionResult<InvoiceListResponseDTO>> GetInvoicesByPeriod(
            int month,
            int year,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Month must be between 1 and 12." });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _invoiceService.GetInvoicesByPeriodAsync(month, year, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving invoices for the period.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates the payment status of an invoice.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <param name="paymentStatusDto">The payment status update data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPatch("{id}/payment")]
        public async Task<ActionResult> UpdateInvoicePaymentStatus(int id, [FromBody] UpdateInvoicePaymentStatusDTO paymentStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _invoiceService.UpdateInvoicePaymentStatusAsync(id, paymentStatusDto);
                if (!success)
                {
                    return NotFound(new { message = $"Invoice with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the payment status.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets invoice statistics.
        /// </summary>
        /// <param name="customerId">Optional customer ID to filter statistics by customer.</param>
        /// <param name="buildingId">Optional building ID to filter statistics by building.</param>
        /// <param name="year">Optional year to filter statistics by year.</param>
        /// <returns>Invoice statistics.</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<InvoiceStatsDTO>> GetInvoiceStats(
            [FromQuery] int? customerId = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? year = null)
        {
            try
            {
                var stats = await _invoiceService.GetInvoiceStatsAsync(customerId, buildingId, year);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving invoice statistics.", details = ex.Message });
            }
        }

        /// <summary>
        /// Checks if an invoice exists.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <returns>True if the invoice exists, false otherwise.</returns>
        [HttpHead("{id}")]
        public async Task<ActionResult> CheckInvoiceExists(int id)
        {
            try
            {
                var exists = await _invoiceService.InvoiceExistsAsync(id);
                if (exists)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking if the invoice exists.", details = ex.Message });
            }
        }

        /// <summary>
        /// Checks if a duplicate invoice exists for the given parameters.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <param name="excludeInvoiceId">Optional invoice ID to exclude from the check.</param>
        /// <returns>True if a duplicate invoice exists, false otherwise.</returns>
        [HttpGet("duplicate-check")]
        public async Task<ActionResult<bool>> CheckDuplicateInvoice(
            [FromQuery] int customerId,
            [FromQuery] int buildingId,
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] int? excludeInvoiceId = null)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Month must be between 1 and 12." });
                }

                var duplicate = await _invoiceService.DuplicateInvoiceExistsAsync(customerId, buildingId, month, year, excludeInvoiceId);
                return Ok(duplicate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking for duplicate invoices.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets the current user's ID from the JWT token.
        /// </summary>
        /// <returns>The current user's ID.</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user token.");
        }
    }
}
