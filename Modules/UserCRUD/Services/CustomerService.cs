using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserCRUD.DTOs;

namespace YopoBackend.Modules.UserCRUD.Services
{
    /// <summary>
    /// Service for managing customer records for property managers.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the CustomerService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new customer record for a property manager user.
        /// </summary>
        /// <param name="user">The user who is a property manager.</param>
        /// <param name="companyName">Optional company name.</param>
        /// <param name="companyLicense">Optional company license number.</param>
        /// <returns>The created customer record if successful; otherwise, null.</returns>
        public async Task<Customer?> CreateCustomerAsync(User user, string? companyName = null, string? companyLicense = null)
        {
            try
            {
                // Only create customer records for Property Managers
                if (user.UserTypeId != UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                {
                    Console.WriteLine($"Customer record creation skipped for user {user.Email} - not a Property Manager (UserTypeId: {user.UserTypeId})");
                    return null;
                }

                // Check if customer record already exists
                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == user.Id);
                if (existingCustomer != null)
                {
                    Console.WriteLine($"Customer record already exists for user {user.Email} (ID: {user.Id})");
                    return existingCustomer;
                }

                // Validate company license uniqueness if provided
                if (!string.IsNullOrEmpty(companyLicense))
                {
                    var licenseExists = await _context.Customers
                        .AnyAsync(c => c.CompanyLicense == companyLicense);
                    if (licenseExists)
                    {
                        throw new ArgumentException($"Company license '{companyLicense}' is already registered by another customer.");
                    }
                }

                // Create new customer record
                var customer = new Customer
                {
                    CustomerId = user.Id, // CustomerId is the same as UserId
                    CustomerName = user.Name, // Map from User.Name
                    CompanyName = companyName,
                    CompanyAddress = user.Address, // Map from User.Address
                    CompanyLicense = companyLicense,
                    Active = true,
                    IsTrial = true,
                    Paid = false,
                    CreatedBy = user.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Customer record created successfully for Property Manager {user.Email} (ID: {user.Id})");
                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating customer record for user {user.Email}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a customer record by user ID.
        /// </summary>
        /// <param name="userId">The user ID (same as CustomerId).</param>
        /// <returns>The customer record if found; otherwise, null.</returns>
        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                        .ThenInclude(u => u!.UserType)
                    .FirstOrDefaultAsync(c => c.CustomerId == userId);

                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer by user ID {userId}: {ex.Message}");
                return null;
            }
        }

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
        public async Task<Customer?> UpdateCustomerAsync(int customerId, string? customerName = null, string? companyName = null, 
            string? companyAddress = null, string? companyLicense = null, bool? active = null, bool? isTrial = null, bool? paid = null)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
                if (customer == null)
                {
                    Console.WriteLine($"Customer not found for ID: {customerId}");
                    return null;
                }

                // Validate company license uniqueness if provided and different from current
                if (!string.IsNullOrEmpty(companyLicense) && companyLicense != customer.CompanyLicense)
                {
                    var licenseExists = await _context.Customers
                        .AnyAsync(c => c.CompanyLicense == companyLicense && c.CustomerId != customerId);
                    if (licenseExists)
                    {
                        throw new ArgumentException($"Company license '{companyLicense}' is already registered by another customer.");
                    }
                }

                // Update fields if provided
                if (customerName != null) customer.CustomerName = customerName;
                if (companyName != null) customer.CompanyName = companyName;
                if (companyAddress != null) customer.CompanyAddress = companyAddress;
                if (companyLicense != null) customer.CompanyLicense = companyLicense;
                if (active.HasValue) customer.Active = active.Value;
                if (isTrial.HasValue) customer.IsTrial = isTrial.Value;
                if (paid.HasValue) customer.Paid = paid.Value;

                customer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"Customer record updated successfully for ID: {customerId}");
                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating customer ID {customerId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes a customer record by customer ID.
        /// </summary>
        /// <param name="customerId">The customer ID to delete.</param>
        /// <returns>True if the customer was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
                if (customer == null)
                {
                    Console.WriteLine($"Customer not found for deletion: {customerId}");
                    return false;
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Customer record deleted successfully for ID: {customerId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer ID {customerId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a user is a customer (property manager).
        /// </summary>
        /// <param name="userId">The user ID to check.</param>
        /// <returns>True if the user is a customer; otherwise, false.</returns>
        public async Task<bool> IsCustomerAsync(int userId)
        {
            try
            {
                return await _context.Customers.AnyAsync(c => c.CustomerId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if user {userId} is a customer: {ex.Message}");
                return false;
            }
        }

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
        public async Task<CustomerListResponseDTO> GetAllCustomersAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? active = null, bool? isTrial = null, bool? paid = null)
        {
            try
            {
                var query = _context.Customers
                    .Include(c => c.User)
                        .ThenInclude(u => u!.UserType)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    query = query.Where(c => c.CustomerName.ToLower().Contains(search) ||
                                           (c.CompanyName != null && c.CompanyName.ToLower().Contains(search)) ||
                                           (c.User != null && c.User.Email.ToLower().Contains(search)));
                }

                if (active.HasValue)
                {
                    query = query.Where(c => c.Active == active.Value);
                }

                if (isTrial.HasValue)
                {
                    query = query.Where(c => c.IsTrial == isTrial.Value);
                }

                if (paid.HasValue)
                {
                    query = query.Where(c => c.Paid == paid.Value);
                }

                var totalCount = await query.CountAsync();

                // Apply pagination
                var customers = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new CustomerListResponseDTO
                {
                    Customers = customers.Select(MapToCustomerResponse).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customers: {ex.Message}");
                return new CustomerListResponseDTO();
            }
        }

        /// <summary>
        /// Updates a customer record using DTO (Super Admin only).
        /// </summary>
        /// <param name="customerId">The customer ID to update.</param>
        /// <param name="updateRequest">The update request DTO.</param>
        /// <returns>The updated customer record if successful; otherwise, null.</returns>
        public async Task<CustomerResponseDTO?> UpdateCustomerAsync(int customerId, UpdateCustomerRequestDTO updateRequest)
        {
            try
            {
                var customer = await UpdateCustomerAsync(
                    customerId,
                    updateRequest.CustomerName,
                    updateRequest.CompanyName,
                    updateRequest.CompanyAddress,
                    updateRequest.CompanyLicense,
                    updateRequest.Active,
                    updateRequest.IsTrial,
                    updateRequest.Paid
                );

                if (customer == null) return null;

                // Reload with user information
                var updatedCustomer = await _context.Customers
                    .Include(c => c.User)
                        .ThenInclude(u => u!.UserType)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                return updatedCustomer != null ? MapToCustomerResponse(updatedCustomer) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating customer {customerId}: {ex.Message}");
                return null;
            }
        }

        // Helper methods

        /// <summary>
        /// Maps a Customer entity to a CustomerResponseDTO.
        /// </summary>
        /// <param name="customer">The customer entity to map.</param>
        /// <returns>The mapped CustomerResponseDTO.</returns>
        private CustomerResponseDTO MapToCustomerResponse(Customer customer)
        {
            return new CustomerResponseDTO
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                CompanyName = customer.CompanyName,
                CompanyAddress = customer.CompanyAddress,
                CompanyLicense = customer.CompanyLicense,
                Active = customer.Active,
                IsTrial = customer.IsTrial,
                Paid = customer.Paid,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                User = customer.User != null ? new CustomerUserInfoDTO
                {
                    Id = customer.User.Id,
                    Email = customer.User.Email,
                    PhoneNumber = customer.User.PhoneNumber,
                    UserTypeName = customer.User.UserType?.Name ?? string.Empty,
                    IsActive = customer.User.IsActive,
                    IsEmailVerified = customer.User.IsEmailVerified,
                    ProfilePhotoBase64 = customer.User.ProfilePhoto != null ? 
                        $"data:{customer.User.ProfilePhotoMimeType};base64,{Convert.ToBase64String(customer.User.ProfilePhoto)}" : null
                } : null
            };
        }
    }
}
