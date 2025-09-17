using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.UserCRUD.DTOs
{
    // Request DTOs

    /// <summary>
    /// DTO for updating customer information.
    /// </summary>
    public class UpdateCustomerRequestDTO
    {
        /// <summary>
        /// Gets or sets the customer name.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        [MaxLength(300, ErrorMessage = "Company name cannot exceed 300 characters")]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Company address cannot exceed 500 characters")]
        public string? CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the company license number.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Company license cannot exceed 100 characters")]
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets whether the customer account is active.
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// Gets or sets whether the customer is on a trial period.
        /// </summary>
        public bool? IsTrial { get; set; }

        /// <summary>
        /// Gets or sets whether the customer has paid for the service.
        /// </summary>
        public bool? Paid { get; set; }
    }

    // Response DTOs

    /// <summary>
    /// DTO for customer response.
    /// </summary>
    public class CustomerResponseDTO
    {
        /// <summary>
        /// Gets or sets the customer ID (same as User ID).
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address.
        /// </summary>
        public string? CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the company license number.
        /// </summary>
        public string? CompanyLicense { get; set; }

        /// <summary>
        /// Gets or sets whether the customer account is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets whether the customer is on a trial period.
        /// </summary>
        public bool IsTrial { get; set; }

        /// <summary>
        /// Gets or sets whether the customer has paid for the service.
        /// </summary>
        public bool Paid { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the customer was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the customer was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the associated user information.
        /// </summary>
        public CustomerUserInfoDTO? User { get; set; }
    }

    /// <summary>
    /// DTO for customer's user information.
    /// </summary>
    public class CustomerUserInfoDTO
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user type name.
        /// </summary>
        public string UserTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets whether the user's email has been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the profile photo as a base64 encoded string.
        /// Will be null if no profile photo is set.
        /// </summary>
        public string? ProfilePhotoBase64 { get; set; }
    }

    /// <summary>
    /// DTO for paginated customer list response.
    /// </summary>
    public class CustomerListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of customers.
        /// </summary>
        public List<CustomerResponseDTO> Customers { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of customers.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Gets or sets whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}