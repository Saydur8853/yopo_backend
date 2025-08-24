using YopoBackend.Modules.InvoiceCRUD.DTOs;

namespace YopoBackend.Modules.InvoiceCRUD.Services
{
    /// <summary>
    /// Interface for invoice service operations.
    /// Module: InvoiceCRUD (Module ID: 7)
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Gets all invoices with pagination support.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="customerId">Optional customer ID to filter invoices by customer.</param>
        /// <param name="buildingId">Optional building ID to filter invoices by building.</param>
        /// <param name="month">Optional month to filter invoices.</param>
        /// <param name="year">Optional year to filter invoices.</param>
        /// <param name="status">Optional status to filter by.</param>
        /// <param name="paidOnly">Whether to include only paid invoices.</param>
        /// <param name="unpaidOnly">Whether to include only unpaid invoices.</param>
        /// <returns>A paginated list of invoices.</returns>
        Task<InvoiceListResponseDTO> GetAllInvoicesAsync(int page = 1, int pageSize = 10, 
            int? customerId = null, int? buildingId = null, int? month = null, int? year = null,
            string? status = null, bool paidOnly = false, bool unpaidOnly = false);

        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <returns>The invoice if found, null otherwise.</returns>
        Task<InvoiceResponseDTO?> GetInvoiceByIdAsync(int invoiceId);

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        /// <param name="createInvoiceDto">The invoice creation data.</param>
        /// <param name="createdByUserId">The ID of the user creating the invoice.</param>
        /// <returns>The created invoice.</returns>
        Task<InvoiceResponseDTO> CreateInvoiceAsync(CreateInvoiceDTO createInvoiceDto, int createdByUserId);

        /// <summary>
        /// Updates an existing invoice.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <param name="updateInvoiceDto">The invoice update data.</param>
        /// <returns>The updated invoice if found, null otherwise.</returns>
        Task<InvoiceResponseDTO?> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceDTO updateInvoiceDto);

        /// <summary>
        /// Deletes an invoice by ID.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <returns>True if the invoice was deleted, false if not found.</returns>
        Task<bool> DeleteInvoiceAsync(int invoiceId);

        /// <summary>
        /// Gets all invoices for a specific customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of invoices for the customer.</returns>
        Task<InvoiceListResponseDTO> GetInvoicesByCustomerIdAsync(int customerId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets all invoices for a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of invoices for the building.</returns>
        Task<InvoiceListResponseDTO> GetInvoicesByBuildingIdAsync(int buildingId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets invoices for a specific month and year.
        /// </summary>
        /// <param name="month">The month (1-12).</param>
        /// <param name="year">The year.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of invoices for the specified period.</returns>
        Task<InvoiceListResponseDTO> GetInvoicesByPeriodAsync(int month, int year, int page = 1, int pageSize = 10);

        /// <summary>
        /// Updates the payment status of an invoice.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <param name="paymentStatusDto">The payment status update data.</param>
        /// <returns>True if the operation was successful, false if invoice not found.</returns>
        Task<bool> UpdateInvoicePaymentStatusAsync(int invoiceId, UpdateInvoicePaymentStatusDTO paymentStatusDto);

        /// <summary>
        /// Gets invoice statistics.
        /// </summary>
        /// <param name="customerId">Optional customer ID to filter statistics by customer.</param>
        /// <param name="buildingId">Optional building ID to filter statistics by building.</param>
        /// <param name="year">Optional year to filter statistics by year.</param>
        /// <returns>Invoice statistics.</returns>
        Task<InvoiceStatsDTO> GetInvoiceStatsAsync(int? customerId = null, int? buildingId = null, int? year = null);

        /// <summary>
        /// Checks if an invoice exists.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <returns>True if the invoice exists, false otherwise.</returns>
        Task<bool> InvoiceExistsAsync(int invoiceId);

        /// <summary>
        /// Checks if an invoice already exists for a customer, building, month, and year combination.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <param name="excludeInvoiceId">Optional invoice ID to exclude from the check (for updates).</param>
        /// <returns>True if a duplicate invoice exists, false otherwise.</returns>
        Task<bool> DuplicateInvoiceExistsAsync(int customerId, int buildingId, int month, int year, int? excludeInvoiceId = null);
    }
}
