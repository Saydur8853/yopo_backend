using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.CustomerCRUD.DTOs;
using YopoBackend.Modules.CustomerCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.CustomerCRUD.Services
{
    /// <summary>
    /// Service implementation for customer operations.
    /// Module: CustomerCRUD (Module ID: 6)
    /// </summary>
    public class CustomerService : BaseAccessControlService, ICustomerService
    {
        /// <summary>
        /// Initializes a new instance of the CustomerService class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public CustomerService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<CustomerListResponseDTO> GetAllCustomersAsync(int page = 1, int pageSize = 10, 
            string? searchTerm = null, int? userId = null, string? type = null, bool activeOnly = false)
        {
            var query = _context.Customers.Include(c => c.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || c.Address.Contains(searchTerm));
            }

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(c => c.Type == type);
            }

            if (activeOnly)
            {
                query = query.Where(c => c.Active);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerResponseDTO
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Address = c.Address,
                    CompanyLicense = c.CompanyLicense,
                    NumberOfUnits = c.NumberOfUnits,
                    Active = c.Active,
                    IsTrial = c.IsTrial,
                    Paid = c.Paid,
                    Type = c.Type,
                    NumberOfTowers = c.NumberOfTowers,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}".Trim() : null,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return new CustomerListResponseDTO
            {
                Customers = customers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        /// <inheritdoc/>
        public async Task<CustomerResponseDTO?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.CustomerId == customerId)
                .Select(c => new CustomerResponseDTO
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Address = c.Address,
                    CompanyLicense = c.CompanyLicense,
                    NumberOfUnits = c.NumberOfUnits,
                    Active = c.Active,
                    IsTrial = c.IsTrial,
                    Paid = c.Paid,
                    Type = c.Type,
                    NumberOfTowers = c.NumberOfTowers,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}".Trim() : null,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return customer;
        }

        /// <inheritdoc/>
        public async Task<CustomerResponseDTO> CreateCustomerAsync(CreateCustomerDTO createCustomerDto, int createdByUserId)
        {
            // Verify that the assigned user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == createCustomerDto.UserId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {createCustomerDto.UserId} does not exist.");
            }

            var customer = new Customer
            {
                Name = createCustomerDto.Name,
                Address = createCustomerDto.Address,
                CompanyLicense = createCustomerDto.CompanyLicense,
                NumberOfUnits = createCustomerDto.NumberOfUnits,
                Active = createCustomerDto.Active,
                IsTrial = createCustomerDto.IsTrial,
                Paid = createCustomerDto.Paid,
                Type = createCustomerDto.Type,
                NumberOfTowers = createCustomerDto.NumberOfTowers,
                UserId = createCustomerDto.UserId,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return await GetCustomerByIdAsync(customer.CustomerId) ?? throw new InvalidOperationException("Failed to retrieve created customer.");
        }

        /// <inheritdoc/>
        public async Task<CustomerResponseDTO?> UpdateCustomerAsync(int customerId, UpdateCustomerDTO updateCustomerDto)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return null;
            }

            // Verify that the assigned user exists if it's being changed
            if (customer.UserId != updateCustomerDto.UserId)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == updateCustomerDto.UserId);
                if (!userExists)
                {
                    throw new ArgumentException($"User with ID {updateCustomerDto.UserId} does not exist.");
                }
            }

            // Update customer properties
            customer.Name = updateCustomerDto.Name;
            customer.Address = updateCustomerDto.Address;
            customer.CompanyLicense = updateCustomerDto.CompanyLicense;
            customer.NumberOfUnits = updateCustomerDto.NumberOfUnits;
            customer.Active = updateCustomerDto.Active;
            customer.IsTrial = updateCustomerDto.IsTrial;
            customer.Paid = updateCustomerDto.Paid;
            customer.Type = updateCustomerDto.Type;
            customer.NumberOfTowers = updateCustomerDto.NumberOfTowers;
            customer.UserId = updateCustomerDto.UserId;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCustomerByIdAsync(customerId);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<CustomerListResponseDTO> GetCustomersByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await GetAllCustomersAsync(page, pageSize, userId: userId);
        }

        /// <inheritdoc/>
        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
        }

        /// <inheritdoc/>
        public async Task<bool> SetCustomerActiveStatusAsync(int customerId, bool isActive)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            customer.Active = isActive;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdatePaymentStatusAsync(int customerId, bool paid)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            customer.Paid = paid;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
