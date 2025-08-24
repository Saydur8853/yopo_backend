using YopoBackend.Modules.CustomerCRUD.DTOs;
using YopoBackend.Modules.CustomerCRUD.Models;

namespace YopoBackend.Modules.CustomerCRUD.Services
{
    /// <summary>
    /// Interface for customer service operations.
    /// Module: CustomerCRUD (Module ID: 6)
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Gets all customers with pagination support.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="searchTerm">Optional search term to filter customers by name or address.</param>
        /// <param name="userId">Optional user ID to filter customers by user.</param>
        /// <param name="type">Optional customer type to filter by.</param>
        /// <param name="activeOnly">Whether to include only active customers.</param>
        /// <returns>A paginated list of customers.</returns>
        Task<CustomerListResponseDTO> GetAllCustomersAsync(int page = 1, int pageSize = 10, 
            string? searchTerm = null, int? userId = null, string? type = null, bool activeOnly = false);

        /// <summary>
        /// Gets a customer by ID.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <returns>The customer if found, null otherwise.</returns>
        Task<CustomerResponseDTO?> GetCustomerByIdAsync(int customerId);

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="createCustomerDto">The customer creation data.</param>
        /// <param name="createdByUserId">The ID of the user creating the customer.</param>
        /// <returns>The created customer.</returns>
        Task<CustomerResponseDTO> CreateCustomerAsync(CreateCustomerDTO createCustomerDto, int createdByUserId);

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="updateCustomerDto">The customer update data.</param>
        /// <returns>The updated customer if found, null otherwise.</returns>
        Task<CustomerResponseDTO?> UpdateCustomerAsync(int customerId, UpdateCustomerDTO updateCustomerDto);

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <returns>True if the customer was deleted, false if not found.</returns>
        Task<bool> DeleteCustomerAsync(int customerId);

        /// <summary>
        /// Gets all customers managed by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of customers managed by the user.</returns>
        Task<CustomerListResponseDTO> GetCustomersByUserIdAsync(int userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Checks if a customer exists.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <returns>True if the customer exists, false otherwise.</returns>
        Task<bool> CustomerExistsAsync(int customerId);

        /// <summary>
        /// Activates or deactivates a customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="isActive">Whether to activate or deactivate the customer.</param>
        /// <returns>True if the operation was successful, false if customer not found.</returns>
        Task<bool> SetCustomerActiveStatusAsync(int customerId, bool isActive);

        /// <summary>
        /// Updates the payment status of a customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="paid">Whether the customer has paid.</param>
        /// <returns>True if the operation was successful, false if customer not found.</returns>
        Task<bool> UpdatePaymentStatusAsync(int customerId, bool paid);
    }
}
