using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserCRUD.DTOs;

namespace YopoBackend.Modules.UserCRUD.Services
{
    /// <summary>
    /// Interface for customer management services.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Creates a new customer record for a property manager user.
        /// </summary>
        /// <param name="user">The user who is a property manager.</param>
        /// <param name="companyName">Optional company name.</param>
        /// <param name="companyLicense">Optional company license number.</param>
        /// <returns>The created customer record if successful; otherwise, null.</returns>
        Task<Customer?> CreateCustomerAsync(User user, string? companyName = null, string? companyLicense = null);

        /// <summary>
        /// Gets a customer record by user ID.
        /// </summary>
        /// <param name="userId">The user ID (same as CustomerId).</param>
        /// <returns>The customer record if found; otherwise, null.</returns>
        Task<Customer?> GetCustomerByUserIdAsync(int userId);

        /// <summary>
        /// Updates an existing customer record.
        /// </summary>
        /// <param name="customerId">The customer ID to update.</param>
        /// <param name="customerName">Updated customer name.</param>
        /// <param name="companyName">Updated company name.</param>
        /// <param name="companyAddress">Updated company address.</param>
        /// <param name="companyLicense">Updated company license.</param>
        /// <param name="active">Updated active status.</param>
        /// <param name="isTrial">Updated trial status.</param>
        /// <param name="paid">Updated paid status.</param>
        /// <returns>The updated customer record if successful; otherwise, null.</returns>
        Task<Customer?> UpdateCustomerAsync(int customerId, string? customerName = null, string? companyName = null, 
            string? companyAddress = null, string? companyLicense = null, bool? active = null, bool? isTrial = null, bool? paid = null);

        /// <summary>
        /// Deletes a customer record by customer ID.
        /// </summary>
        /// <param name="customerId">The customer ID to delete.</param>
        /// <returns>True if the customer was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteCustomerAsync(int customerId);

        /// <summary>
        /// Checks if a user is a customer (property manager).
        /// </summary>
        /// <param name="userId">The user ID to check.</param>
        /// <returns>True if the user is a customer; otherwise, false.</returns>
        Task<bool> IsCustomerAsync(int userId);

        // Admin methods

        /// <summary>
        /// Gets all customers with pagination and filtering (Super Admin only).
        /// </summary>
        /// <param name="page">The page number (starting from 1).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="searchTerm">Optional search term to filter customers by name, company, or email.</param>
        /// <param name="active">Optional active status filter.</param>
        /// <param name="isTrial">Optional trial status filter.</param>
        /// <param name="paid">Optional paid status filter.</param>
        /// <returns>A paginated list of customers.</returns>
        Task<CustomerListResponseDTO> GetAllCustomersAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? active = null, bool? isTrial = null, bool? paid = null);

        /// <summary>
        /// Updates a customer record using DTO (Super Admin only).
        /// </summary>
        /// <param name="customerId">The customer ID to update.</param>
        /// <param name="updateRequest">The update request DTO.</param>
        /// <returns>The updated customer record if successful; otherwise, null.</returns>
        Task<CustomerResponseDTO?> UpdateCustomerAsync(int customerId, UpdateCustomerRequestDTO updateRequest);
    }
}
