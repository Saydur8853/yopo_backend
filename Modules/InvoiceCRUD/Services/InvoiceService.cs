using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.InvoiceCRUD.DTOs;
using YopoBackend.Modules.InvoiceCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.InvoiceCRUD.Services
{
    /// <summary>
    /// Service implementation for invoice operations.
    /// Module: InvoiceCRUD (Module ID: 7)
    /// </summary>
    public class InvoiceService : BaseAccessControlService, IInvoiceService
    {
        /// <summary>
        /// Initializes a new instance of the InvoiceService class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public InvoiceService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<InvoiceListResponseDTO> GetAllInvoicesAsync(int page = 1, int pageSize = 10, 
            int? customerId = null, int? buildingId = null, int? month = null, int? year = null,
            string? status = null, bool paidOnly = false, bool unpaidOnly = false)
        {
            var query = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Building)
                .AsQueryable();

            // Apply filters
            if (customerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == customerId.Value);
            }

            if (buildingId.HasValue)
            {
                query = query.Where(i => i.BuildingId == buildingId.Value);
            }

            if (month.HasValue)
            {
                query = query.Where(i => i.Month == month.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(i => i.Year == year.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (paidOnly)
            {
                query = query.Where(i => i.Paid);
            }
            else if (unpaidOnly)
            {
                query = query.Where(i => !i.Paid);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var invoices = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InvoiceResponseDTO
                {
                    InvoiceId = i.InvoiceId,
                    CustomerId = i.CustomerId,
                    CustomerName = i.Customer != null ? i.Customer.Name : null,
                    Month = i.Month,
                    Year = i.Year,
                    BuildingId = i.BuildingId,
                    BuildingName = i.Building != null ? i.Building.Name : null,
                    Paid = i.Paid,
                    Status = i.Status,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Description = i.Description,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return new InvoiceListResponseDTO
            {
                Invoices = invoices,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        /// <inheritdoc/>
        public async Task<InvoiceResponseDTO?> GetInvoiceByIdAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Building)
                .Where(i => i.InvoiceId == invoiceId)
                .Select(i => new InvoiceResponseDTO
                {
                    InvoiceId = i.InvoiceId,
                    CustomerId = i.CustomerId,
                    CustomerName = i.Customer != null ? i.Customer.Name : null,
                    Month = i.Month,
                    Year = i.Year,
                    BuildingId = i.BuildingId,
                    BuildingName = i.Building != null ? i.Building.Name : null,
                    Paid = i.Paid,
                    Status = i.Status,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Description = i.Description,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return invoice;
        }

        /// <inheritdoc/>
        public async Task<InvoiceResponseDTO> CreateInvoiceAsync(CreateInvoiceDTO createInvoiceDto, int createdByUserId)
        {
            // Verify that the customer exists
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == createInvoiceDto.CustomerId);
            if (!customerExists)
            {
                throw new ArgumentException($"Customer with ID {createInvoiceDto.CustomerId} does not exist.");
            }

            // Verify that the building exists
            var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == createInvoiceDto.BuildingId);
            if (!buildingExists)
            {
                throw new ArgumentException($"Building with ID {createInvoiceDto.BuildingId} does not exist.");
            }

            // Check for duplicate invoice
            var duplicateExists = await DuplicateInvoiceExistsAsync(
                createInvoiceDto.CustomerId, 
                createInvoiceDto.BuildingId, 
                createInvoiceDto.Month, 
                createInvoiceDto.Year
            );
            if (duplicateExists)
            {
                throw new ArgumentException($"An invoice already exists for Customer {createInvoiceDto.CustomerId}, Building {createInvoiceDto.BuildingId}, {createInvoiceDto.Month}/{createInvoiceDto.Year}.");
            }

            var invoice = new Invoice
            {
                CustomerId = createInvoiceDto.CustomerId,
                Month = createInvoiceDto.Month,
                Year = createInvoiceDto.Year,
                BuildingId = createInvoiceDto.BuildingId,
                Paid = createInvoiceDto.Paid,
                Status = createInvoiceDto.Status,
                Amount = createInvoiceDto.Amount,
                DueDate = createInvoiceDto.DueDate,
                Description = createInvoiceDto.Description,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return await GetInvoiceByIdAsync(invoice.InvoiceId) ?? throw new InvalidOperationException("Failed to retrieve created invoice.");
        }

        /// <inheritdoc/>
        public async Task<InvoiceResponseDTO?> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceDTO updateInvoiceDto)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return null;
            }

            // Verify that the customer exists if it's being changed
            if (invoice.CustomerId != updateInvoiceDto.CustomerId)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == updateInvoiceDto.CustomerId);
                if (!customerExists)
                {
                    throw new ArgumentException($"Customer with ID {updateInvoiceDto.CustomerId} does not exist.");
                }
            }

            // Verify that the building exists if it's being changed
            if (invoice.BuildingId != updateInvoiceDto.BuildingId)
            {
                var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == updateInvoiceDto.BuildingId);
                if (!buildingExists)
                {
                    throw new ArgumentException($"Building with ID {updateInvoiceDto.BuildingId} does not exist.");
                }
            }

            // Check for duplicate invoice if key fields are changing
            if (invoice.CustomerId != updateInvoiceDto.CustomerId || 
                invoice.BuildingId != updateInvoiceDto.BuildingId ||
                invoice.Month != updateInvoiceDto.Month || 
                invoice.Year != updateInvoiceDto.Year)
            {
                var duplicateExists = await DuplicateInvoiceExistsAsync(
                    updateInvoiceDto.CustomerId, 
                    updateInvoiceDto.BuildingId, 
                    updateInvoiceDto.Month, 
                    updateInvoiceDto.Year,
                    invoiceId
                );
                if (duplicateExists)
                {
                    throw new ArgumentException($"An invoice already exists for Customer {updateInvoiceDto.CustomerId}, Building {updateInvoiceDto.BuildingId}, {updateInvoiceDto.Month}/{updateInvoiceDto.Year}.");
                }
            }

            // Update invoice properties
            invoice.CustomerId = updateInvoiceDto.CustomerId;
            invoice.Month = updateInvoiceDto.Month;
            invoice.Year = updateInvoiceDto.Year;
            invoice.BuildingId = updateInvoiceDto.BuildingId;
            invoice.Paid = updateInvoiceDto.Paid;
            invoice.Status = updateInvoiceDto.Status;
            invoice.Amount = updateInvoiceDto.Amount;
            invoice.DueDate = updateInvoiceDto.DueDate;
            invoice.Description = updateInvoiceDto.Description;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetInvoiceByIdAsync(invoiceId);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<InvoiceListResponseDTO> GetInvoicesByCustomerIdAsync(int customerId, int page = 1, int pageSize = 10)
        {
            return await GetAllInvoicesAsync(page, pageSize, customerId: customerId);
        }

        /// <inheritdoc/>
        public async Task<InvoiceListResponseDTO> GetInvoicesByBuildingIdAsync(int buildingId, int page = 1, int pageSize = 10)
        {
            return await GetAllInvoicesAsync(page, pageSize, buildingId: buildingId);
        }

        /// <inheritdoc/>
        public async Task<InvoiceListResponseDTO> GetInvoicesByPeriodAsync(int month, int year, int page = 1, int pageSize = 10)
        {
            return await GetAllInvoicesAsync(page, pageSize, month: month, year: year);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateInvoicePaymentStatusAsync(int invoiceId, UpdateInvoicePaymentStatusDTO paymentStatusDto)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            invoice.Paid = paymentStatusDto.Paid;
            
            // Update status based on payment status
            if (paymentStatusDto.Paid)
            {
                invoice.Status = "Paid";
            }
            else if (invoice.Status == "Paid")
            {
                invoice.Status = "Sent"; // Reset to sent if unpaid
            }
            
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<InvoiceStatsDTO> GetInvoiceStatsAsync(int? customerId = null, int? buildingId = null, int? year = null)
        {
            var query = _context.Invoices.AsQueryable();

            // Apply filters
            if (customerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == customerId.Value);
            }

            if (buildingId.HasValue)
            {
                query = query.Where(i => i.BuildingId == buildingId.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(i => i.Year == year.Value);
            }

            var totalInvoices = await query.CountAsync();
            var paidInvoices = await query.CountAsync(i => i.Paid);
            var unpaidInvoices = totalInvoices - paidInvoices;
            
            // Count overdue invoices (unpaid and past due date)
            var overdueInvoices = await query
                .CountAsync(i => !i.Paid && i.DueDate.HasValue && i.DueDate.Value < DateTime.UtcNow);

            var totalAmount = await query.SumAsync(i => i.Amount ?? 0);
            var paidAmount = await query.Where(i => i.Paid).SumAsync(i => i.Amount ?? 0);
            var unpaidAmount = totalAmount - paidAmount;

            return new InvoiceStatsDTO
            {
                TotalInvoices = totalInvoices,
                PaidInvoices = paidInvoices,
                UnpaidInvoices = unpaidInvoices,
                OverdueInvoices = overdueInvoices,
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                UnpaidAmount = unpaidAmount
            };
        }

        /// <inheritdoc/>
        public async Task<bool> InvoiceExistsAsync(int invoiceId)
        {
            return await _context.Invoices.AnyAsync(i => i.InvoiceId == invoiceId);
        }

        /// <inheritdoc/>
        public async Task<bool> DuplicateInvoiceExistsAsync(int customerId, int buildingId, int month, int year, int? excludeInvoiceId = null)
        {
            var query = _context.Invoices.Where(i => 
                i.CustomerId == customerId && 
                i.BuildingId == buildingId && 
                i.Month == month && 
                i.Year == year);

            if (excludeInvoiceId.HasValue)
            {
                query = query.Where(i => i.InvoiceId != excludeInvoiceId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
