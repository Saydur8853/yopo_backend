using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.CustomerCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new customer.
    /// </summary>
    public class CreateCustomerDTO
    {
        /// <summary>
        /// Gets or sets the customer name.
        /// </summary>
        [Required]
        [StringLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer address.
        /// </summary>
        [Required]
        [StringLength(500, ErrorMessage = "Address must not exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company license number or identifier.
        /// </summary>
        [StringLength(100, ErrorMessage = "Company license must not exceed 100 characters.")]
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets the number of units this customer manages.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Number of units must be non-negative.")]
        public int NumberOfUnits { get; set; } = 0;

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
        [StringLength(50, ErrorMessage = "Type must not exceed 50 characters.")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of towers/buildings this customer manages.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Number of towers must be non-negative.")]
        public int NumberOfTowers { get; set; } = 0;

    }

    /// <summary>
    /// DTO for updating an existing customer.
    /// </summary>
    public class UpdateCustomerDTO
    {
        /// <summary>
        /// Gets or sets the customer name.
        /// </summary>
        [Required]
        [StringLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer address.
        /// </summary>
        [Required]
        [StringLength(500, ErrorMessage = "Address must not exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company license number or identifier.
        /// </summary>
        [StringLength(100, ErrorMessage = "Company license must not exceed 100 characters.")]
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets the number of units this customer manages.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Number of units must be non-negative.")]
        public int NumberOfUnits { get; set; } = 0;

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
        [StringLength(50, ErrorMessage = "Type must not exceed 50 characters.")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of towers/buildings this customer manages.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Number of towers must be non-negative.")]
        public int NumberOfTowers { get; set; } = 0;

    }

    /// <summary>
    /// DTO for customer response data.
    /// </summary>
    public class CustomerResponseDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company license number or identifier.
        /// </summary>
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets the number of units this customer manages.
        /// </summary>
        public int NumberOfUnits { get; set; }

        /// <summary>
        /// Gets or sets whether the customer account is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets whether this customer is on a trial period.
        /// </summary>
        public bool IsTrial { get; set; }

        /// <summary>
        /// Gets or sets whether the customer has paid their fees.
        /// </summary>
        public bool Paid { get; set; }

        /// <summary>
        /// Gets or sets the customer type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of towers/buildings this customer manages.
        /// </summary>
        public int NumberOfTowers { get; set; }

        /// <summary>
        /// Gets or sets the user ID who manages this customer account.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user name who manages this customer account.
        /// </summary>
        public string? UserName { get; set; }

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
    /// DTO for customer list response with pagination support.
    /// </summary>
    public class CustomerListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of customers.
        /// </summary>
        public List<CustomerResponseDTO> Customers { get; set; } = new List<CustomerResponseDTO>();

        /// <summary>
        /// Gets or sets the total count of customers.
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
}
