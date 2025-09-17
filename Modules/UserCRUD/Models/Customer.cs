using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.UserCRUD.Models
{
    /// <summary>
    /// Represents a customer (property manager) in the system with company information.
    /// This table stores additional business information for users registered as property managers.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    [Table("Customers")]
    public class Customer : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// This is also a foreign key referencing the Users table.
        /// </summary>
        [Key]
        [Column("CustomerId")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name (copied from User.Name when property manager registers).
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("CustomerName")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company name of the property management business.
        /// </summary>
        [MaxLength(300)]
        [Column("CompanyName")]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address (copied from User.Address when property manager registers).
        /// </summary>
        [MaxLength(500)]
        [Column("CompanyAddress")]
        public string? CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the company license number for the property management business.
        /// </summary>
        [MaxLength(100)]
        [Column("CompanyLicense")]
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets whether the customer account is active.
        /// </summary>
        [Column("Active")]
        public bool Active { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the customer is on a trial period.
        /// </summary>
        [Column("IsTrial")]
        public bool IsTrial { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the customer has paid for the service.
        /// </summary>
        [Column("Paid")]
        public bool Paid { get; set; } = false;

        /// <summary>
        /// Gets or sets the ID of the user who created this customer record.
        /// </summary>
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the customer record was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the customer record was last updated.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user associated with this customer.
        /// </summary>
        [ForeignKey("CustomerId")]
        public virtual User? User { get; set; }
    }
}