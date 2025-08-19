using System.Linq;
using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.TenantCRUD.DTOs;
using YopoBackend.Modules.TenantCRUD.Models;

namespace YopoBackend.Modules.TenantCRUD.Services
{
    /// <summary>
    /// Service for managing tenant operations.
    /// Module ID: 5 (TenantCRUD)
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the TenantService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<TenantListDto> GetAllTenantsAsync(TenantSearchDto searchDto)
        {
            var query = _context.Tenants
                .Include(t => t.Building)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchDto.Name))
            {
                query = query.Where(t => t.Name.Contains(searchDto.Name));
            }

            if (searchDto.BuildingId.HasValue)
            {
                query = query.Where(t => t.BuildingId == searchDto.BuildingId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Type))
            {
                query = query.Where(t => t.Type.Contains(searchDto.Type));
            }

            if (searchDto.Floor.HasValue)
            {
                query = query.Where(t => t.Floor == searchDto.Floor.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.UnitNo))
            {
                query = query.Where(t => t.UnitNo.Contains(searchDto.UnitNo));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.MemberType))
            {
                query = query.Where(t => t.MemberType.Contains(searchDto.MemberType));
            }

            if (searchDto.Paid.HasValue)
            {
                query = query.Where(t => t.Paid == searchDto.Paid.Value);
            }

            if (searchDto.Active.HasValue)
            {
                query = query.Where(t => t.Active == searchDto.Active.Value);
            }

            if (searchDto.ContractStartDateFrom.HasValue)
            {
                query = query.Where(t => t.ContractStartDate >= searchDto.ContractStartDateFrom.Value);
            }

            if (searchDto.ContractStartDateTo.HasValue)
            {
                query = query.Where(t => t.ContractStartDate <= searchDto.ContractStartDateTo.Value);
            }

            if (searchDto.ContractEndDateFrom.HasValue)
            {
                query = query.Where(t => t.ContractEndDate >= searchDto.ContractEndDateFrom.Value);
            }

            if (searchDto.ContractEndDateTo.HasValue)
            {
                query = query.Where(t => t.ContractEndDate <= searchDto.ContractEndDateTo.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tenants = await query
                .OrderBy(t => t.Name)
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(t => MapToDto(t))
                .ToListAsync();

            return new TenantListDto
            {
                Tenants = tenants,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TenantDto>> GetActiveTenantsAsync()
        {
            return await _context.Tenants
                .Include(t => t.Building)
                .Where(t => t.Active)
                .OrderBy(t => t.Name)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<TenantDto?> GetTenantByIdAsync(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Building)
                .FirstOrDefaultAsync(t => t.TenantId == id);

            return tenant == null ? null : MapToDto(tenant);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TenantDto>> GetTenantsByBuildingIdAsync(int buildingId)
        {
            return await _context.Tenants
                .Include(t => t.Building)
                .Where(t => t.BuildingId == buildingId)
                .OrderBy(t => t.Floor)
                .ThenBy(t => t.UnitNo)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TenantDto>> GetTenantsByFloorAsync(int buildingId, int floor)
        {
            return await _context.Tenants
                .Include(t => t.Building)
                .Where(t => t.BuildingId == buildingId && t.Floor == floor)
                .OrderBy(t => t.UnitNo)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<TenantDto> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            // Check if unit already exists in the building
            if (await TenantExistsInUnitAsync(createTenantDto.BuildingId, createTenantDto.UnitNo))
            {
                throw new InvalidOperationException($"A tenant already exists in unit {createTenantDto.UnitNo} in this building.");
            }

            // Verify building exists
            var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == createTenantDto.BuildingId);
            if (!buildingExists)
            {
                throw new InvalidOperationException($"Building with ID {createTenantDto.BuildingId} does not exist.");
            }

            var tenant = new Tenant
            {
                Name = createTenantDto.Name,
                BuildingId = createTenantDto.BuildingId,
                Type = createTenantDto.Type,
                Floor = createTenantDto.Floor,
                UnitNo = createTenantDto.UnitNo,
                ParkingSpace = createTenantDto.ParkingSpace,
                Contact = createTenantDto.Contact,
                ContractStartDate = createTenantDto.ContractStartDate,
                ContractEndDate = createTenantDto.ContractEndDate,
                Paid = createTenantDto.Paid,
                MemberType = createTenantDto.MemberType,
                Files = createTenantDto.Files,
                Active = createTenantDto.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return await GetTenantByIdAsync(tenant.TenantId) ?? throw new InvalidOperationException("Failed to retrieve created tenant.");
        }

        /// <inheritdoc />
        public async Task<TenantDto?> UpdateTenantAsync(int id, UpdateTenantDto updateTenantDto)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return null;
            }

            // Check if unit change conflicts with existing tenant
            if (updateTenantDto.BuildingId.HasValue && updateTenantDto.UnitNo != null)
            {
                if (await TenantExistsInUnitAsync(updateTenantDto.BuildingId.Value, updateTenantDto.UnitNo, id))
                {
                    throw new InvalidOperationException($"A tenant already exists in unit {updateTenantDto.UnitNo} in this building.");
                }

                // Verify building exists
                var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == updateTenantDto.BuildingId.Value);
                if (!buildingExists)
                {
                    throw new InvalidOperationException($"Building with ID {updateTenantDto.BuildingId.Value} does not exist.");
                }
            }

            // Update only provided fields
            if (updateTenantDto.Name != null)
                tenant.Name = updateTenantDto.Name;

            if (updateTenantDto.BuildingId.HasValue)
                tenant.BuildingId = updateTenantDto.BuildingId.Value;

            if (updateTenantDto.Type != null)
                tenant.Type = updateTenantDto.Type;

            if (updateTenantDto.Floor.HasValue)
                tenant.Floor = updateTenantDto.Floor.Value;

            if (updateTenantDto.UnitNo != null)
                tenant.UnitNo = updateTenantDto.UnitNo;

            if (updateTenantDto.ParkingSpace.HasValue)
                tenant.ParkingSpace = updateTenantDto.ParkingSpace.Value;

            if (updateTenantDto.Contact != null)
                tenant.Contact = updateTenantDto.Contact;

            if (updateTenantDto.ContractStartDate.HasValue)
                tenant.ContractStartDate = updateTenantDto.ContractStartDate.Value;

            if (updateTenantDto.ContractEndDate.HasValue)
                tenant.ContractEndDate = updateTenantDto.ContractEndDate.Value;

            if (updateTenantDto.Paid.HasValue)
                tenant.Paid = updateTenantDto.Paid.Value;

            if (updateTenantDto.MemberType != null)
                tenant.MemberType = updateTenantDto.MemberType;

            if (updateTenantDto.Files != null)
                tenant.Files = updateTenantDto.Files;

            if (updateTenantDto.Active.HasValue)
                tenant.Active = updateTenantDto.Active.Value;

            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetTenantByIdAsync(id);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTenantAsync(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return false;
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TenantExistsInUnitAsync(int buildingId, string unitNo, int? excludeId = null)
        {
            var query = _context.Tenants
                .Where(t => t.BuildingId == buildingId && t.UnitNo == unitNo && t.Active);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.TenantId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TenantDto>> GetTenantsWithExpiringContractsAsync(int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);

            return await _context.Tenants
                .Include(t => t.Building)
                .Where(t => t.Active && t.ContractEndDate <= cutoffDate && t.ContractEndDate >= DateTime.UtcNow)
                .OrderBy(t => t.ContractEndDate)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TenantDto>> GetUnpaidTenantsAsync()
        {
            return await _context.Tenants
                .Include(t => t.Building)
                .Where(t => t.Active && !t.Paid)
                .OrderBy(t => t.Name)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> UpdatePaymentStatusAsync(int id, bool paid)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return false;
            }

            tenant.Paid = paid;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<object> GetTenantStatisticsAsync(int? buildingId = null)
        {
            var query = _context.Tenants.AsQueryable();

            if (buildingId.HasValue)
            {
                query = query.Where(t => t.BuildingId == buildingId.Value);
            }

            var stats = await query
                .GroupBy(t => 1)
                .Select(g => new
                {
                    TotalTenants = g.Count(),
                    ActiveTenants = g.Count(t => t.Active),
                    InactiveTenants = g.Count(t => !t.Active),
                    PaidTenants = g.Count(t => t.Paid),
                    UnpaidTenants = g.Count(t => !t.Paid),
                    ExpiringContracts = g.Count(t => t.ContractEndDate <= DateTime.UtcNow.AddDays(30) && t.ContractEndDate >= DateTime.UtcNow),
                    TenantsByType = g.GroupBy(t => t.Type).Select(tg => new { Type = tg.Key, Count = tg.Count() }),
                    TenantsByMemberType = g.GroupBy(t => t.MemberType).Select(mg => new { MemberType = mg.Key, Count = mg.Count() })
                })
                .FirstOrDefaultAsync();

            return stats ?? new
            {
                TotalTenants = 0,
                ActiveTenants = 0,
                InactiveTenants = 0,
                PaidTenants = 0,
                UnpaidTenants = 0,
                ExpiringContracts = 0,
                TenantsByType = Enumerable.Empty<object>().Select(x => new { Type = (string)null!, Count = 0 }),
                TenantsByMemberType = Enumerable.Empty<object>().Select(x => new { MemberType = (string)null!, Count = 0 })
            };
        }

        /// <summary>
        /// Maps a Tenant entity to a TenantDto.
        /// </summary>
        /// <param name="tenant">The tenant entity to map.</param>
        /// <returns>The mapped tenant DTO.</returns>
        private static TenantDto MapToDto(Tenant tenant)
        {
            return new TenantDto
            {
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                BuildingId = tenant.BuildingId,
                BuildingName = tenant.Building?.Name ?? "Unknown Building",
                Type = tenant.Type,
                Floor = tenant.Floor,
                UnitNo = tenant.UnitNo,
                ParkingSpace = tenant.ParkingSpace,
                Contact = tenant.Contact,
                ContractStartDate = tenant.ContractStartDate,
                ContractEndDate = tenant.ContractEndDate,
                Paid = tenant.Paid,
                MemberType = tenant.MemberType,
                Files = tenant.Files,
                Active = tenant.Active,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };
        }
    }
}
