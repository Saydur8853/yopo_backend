using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.InvoiceCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new invoice.
    /// </summary>
    public class CreateInvoiceDTO
    {
        /// <summary>
        /// Gets or sets the customer ID associated with this invoice.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be a positive integer.")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the month for which this invoice is generated.
        /// </summary>
        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the year for which this invoice is generated.
        /// </summary>
        [Required]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100.")]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the building ID for which this invoice is generated.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Building ID must be a positive integer.")]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets whether this invoice has been paid.
        /// </summary>
        public bool Paid { get; set; } = false;

        /// <summary>
        /// Gets or sets the status of the invoice.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the due date for this invoice.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or description for the invoice.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing invoice.
    /// </summary>
    public class UpdateInvoiceDTO
    {
        /// <summary>
        /// Gets or sets the customer ID associated with this invoice.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be a positive integer.")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the month for which this invoice is generated.
        /// </summary>
        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the year for which this invoice is generated.
        /// </summary>
        [Required]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100.")]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the building ID for which this invoice is generated.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Building ID must be a positive integer.")]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets whether this invoice has been paid.
        /// </summary>
        public bool Paid { get; set; }

        /// <summary>
        /// Gets or sets the status of the invoice.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the due date for this invoice.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or description for the invoice.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for invoice response data.
    /// </summary>
    public class InvoiceResponseDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invoice.
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID associated with this invoice.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name associated with this invoice.
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the month for which this invoice is generated.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the year for which this invoice is generated.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the building ID for which this invoice is generated.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the building name for which this invoice is generated.
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets whether this invoice has been paid.
        /// </summary>
        public bool Paid { get; set; }

        /// <summary>
        /// Gets or sets the status of the invoice.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the due date for this invoice.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or description for the invoice.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for paginated list of invoices.
    /// </summary>
    public class InvoiceListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of invoices.
        /// </summary>
        public List<InvoiceResponseDTO> Invoices { get; set; } = new List<InvoiceResponseDTO>();

        /// <summary>
        /// Gets or sets the total count of invoices.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DTO for updating invoice payment status.
    /// </summary>
    public class UpdateInvoicePaymentStatusDTO
    {
        /// <summary>
        /// Gets or sets whether the invoice has been paid.
        /// </summary>
        [Required]
        public bool Paid { get; set; }

        /// <summary>
        /// Gets or sets the payment date (optional).
        /// </summary>
        public DateTime? PaymentDate { get; set; }
    }

    /// <summary>
    /// DTO for invoice statistics.
    /// </summary>
    public class InvoiceStatsDTO
    {
        /// <summary>
        /// Gets or sets the total number of invoices.
        /// </summary>
        public int TotalInvoices { get; set; }

        /// <summary>
        /// Gets or sets the number of paid invoices.
        /// </summary>
        public int PaidInvoices { get; set; }

        /// <summary>
        /// Gets or sets the number of unpaid invoices.
        /// </summary>
        public int UnpaidInvoices { get; set; }

        /// <summary>
        /// Gets or sets the number of overdue invoices.
        /// </summary>
        public int OverdueInvoices { get; set; }

        /// <summary>
        /// Gets or sets the total amount of all invoices.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the total amount of paid invoices.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Gets or sets the total amount of unpaid invoices.
        /// </summary>
        public decimal UnpaidAmount { get; set; }
    }
}
