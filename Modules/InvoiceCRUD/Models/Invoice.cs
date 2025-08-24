using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.InvoiceCRUD.Models
{
    /// <summary>
    /// Represents an invoice entity in the system.
    /// Module: InvoiceCRUD (Module ID: 7)
    /// </summary>
    [Table("Invoices")]
    public class Invoice : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invoice.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID associated with this invoice.
        /// </summary>
        [Required]
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
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets whether this invoice has been paid.
        /// </summary>
        public bool Paid { get; set; } = false;

        /// <summary>
        /// Gets or sets the status of the invoice (e.g., Draft, Sent, Paid, Overdue, Cancelled).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Gets or sets the invoice amount (optional for future use).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the due date for this invoice.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or description for the invoice.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this invoice record.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation property to the customer associated with this invoice.
        /// </summary>
        public virtual YopoBackend.Modules.CustomerCRUD.Models.Customer? Customer { get; set; }

        /// <summary>
        /// Navigation property to the building associated with this invoice.
        /// </summary>
        public virtual YopoBackend.Modules.BuildingCRUD.Models.Building? Building { get; set; }
    }
}
