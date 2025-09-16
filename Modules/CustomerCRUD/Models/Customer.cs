using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.CustomerCRUD.Models
{
    /// <summary>
    /// Represents a customer entity in the system.
    /// Module: CustomerCRUD (Module ID: 6)
    /// </summary>
    [Table("Customers")]
    public class Customer : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer address.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company license number or identifier.
        /// </summary>
        [StringLength(100)]
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        [StringLength(250)]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets whether the customer account is active.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this customer is on a trial period.
        /// </summary>
        public bool IsTrial { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the customer has paid their fees.
        /// </summary>
        public bool Paid { get; set; } = false;

        /// <summary>
        /// Gets or sets the customer type (e.g., Individual, Corporate, Enterprise).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user ID who manages this customer account.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this customer record.
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
        /// Navigation property to the user who manages this customer.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual YopoBackend.Modules.UserCRUD.Models.User? User { get; set; }

        /// <summary>
        /// Navigation property to the invoices created for this customer.
        /// </summary>
        public virtual ICollection<YopoBackend.Modules.InvoiceCRUD.Models.Invoice> Invoices { get; set; } = new List<YopoBackend.Modules.InvoiceCRUD.Models.Invoice>();
    }
}
