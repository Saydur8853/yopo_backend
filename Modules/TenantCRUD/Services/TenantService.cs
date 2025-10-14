using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.TenantCRUD.DTOs;
using YopoBackend.Modules.TenantCRUD.Models;
using YopoBackend.Services;
using YopoBackend.Constants;
using YopoBackend.Modules.InvitationCRUD.Services;
using YopoBackend.Modules.InvitationCRUD.DTOs;

namespace YopoBackend.Modules.TenantCRUD.Services
{
    /// <summary>
    /// Service for managing Tenant records with PM data access control.
    /// </summary>
    public class TenantService : BaseAccessControlService, ITenantService
    {
        private readonly IInvitationService _invitationService;

        public TenantService(ApplicationDbContext context, IInvitationService invitationService) : base(context)
        {
            _invitationService = invitationService;
        }

        public async Task<(List<TenantResponseDTO> tenants, int totalRecords)> GetTenantsAsync(int currentUserId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? buildingId = null, int? floorId = null, int? unitId = null, bool? isActive = null, bool? isPaid = null)
        {
            var query = _context.Tenants
                .Include(t => t.Building)
                .Include(t => t.Floor)
                .Include(t => t.Unit)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            // Apply access control (PM ecosystem / OWN / ALL)
            query = await ApplyAccessControlAsync(query, currentUserId);

            // If the user is a non-PM invited user with explicit building permissions, restrict to those buildings
            var explicitBuildingIds = await _context.UserBuildingPermissions
                .Where(p => p.UserId == currentUserId && p.IsActive)
                .Select(p => p.BuildingId)
                .ToListAsync();
            if (explicitBuildingIds.Any())
            {
                query = query.Where(t => explicitBuildingIds.Contains(t.BuildingId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.TenantName.Contains(searchTerm) || 
                                        t.Building.Name.Contains(searchTerm) ||
                                        (t.Floor != null && t.Floor.Name.Contains(searchTerm)) ||
                                        (t.Unit != null && t.Unit.UnitNumber.Contains(searchTerm)));
            }
            if (buildingId.HasValue) query = query.Where(t => t.BuildingId == buildingId.Value);
            if (floorId.HasValue) query = query.Where(t => t.FloorId == floorId.Value);
            if (unitId.HasValue) query = query.Where(t => t.UnitId == unitId.Value);
            if (isActive.HasValue) query = query.Where(t => t.IsActive == isActive.Value);
            if (isPaid.HasValue) query = query.Where(t => t.IsPaid == isPaid.Value);

            var totalRecords = await query.CountAsync();

            var tenants = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TenantResponseDTO
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    BuildingId = t.BuildingId,
                    BuildingName = t.Building.Name,
                    Type = t.Type,
                    FloorId = t.FloorId,
                    FloorName = t.Floor != null ? t.Floor.Name : null,
                    UnitId = t.UnitId,
                    UnitNumber = t.Unit != null ? t.Unit.UnitNumber : null,
                    ContractStartDate = t.ContractStartDate,
                    ContractEndDate = t.ContractEndDate,
                    IsPaid = t.IsPaid,
                    MemberType = t.MemberType,
                    DocumentFile = t.DocumentFile,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    CreatedByName = t.CreatedByUser.Name
                })
                .ToListAsync();

            return (tenants, totalRecords);
        }

        public async Task<TenantResponseDTO> CreateTenantAsync(CreateTenantDTO dto, int currentUserId)
        {
            // Validate building exists and is accessible (either PM ecosystem created or explicit permission)
            var buildingExists = await _context.Buildings.AnyAsync(b => b.BuildingId == dto.BuildingId);
            if (!buildingExists)
                throw new ArgumentException("Invalid BuildingId");

            // Validate FloorId belongs to the building if provided
            if (dto.FloorId.HasValue)
            {
                var floorExists = await _context.Floors.AnyAsync(f => f.FloorId == dto.FloorId.Value);
                if (!floorExists)
                    throw new ArgumentException("Invalid FloorId");

                var floorBelongsToBuilding = await _context.Floors
                    .AnyAsync(f => f.FloorId == dto.FloorId.Value && f.BuildingId == dto.BuildingId);
                if (!floorBelongsToBuilding)
                    throw new ArgumentException("FloorId does not belong to the specified BuildingId");
            }

            // Validate UnitId belongs to the building (and floor if provided) if provided
            if (dto.UnitId.HasValue)
            {
                var unitQuery = _context.Units.Where(u => u.UnitId == dto.UnitId.Value && u.BuildingId == dto.BuildingId);
                if (dto.FloorId.HasValue)
                {
                    unitQuery = unitQuery.Where(u => u.FloorId == dto.FloorId.Value);
                }
                var unitBelongsToBuilding = await unitQuery.AnyAsync();
                if (!unitBelongsToBuilding)
                    throw new ArgumentException("UnitId does not belong to the specified BuildingId/FloorId");
            }

            var entity = new Tenant
            {
                TenantName = dto.TenantName,
                BuildingId = dto.BuildingId,
                Type = dto.Type,
                FloorId = dto.FloorId,
                UnitId = dto.UnitId,
                ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate,
                IsPaid = dto.IsPaid,
                MemberType = dto.MemberType,
                DocumentFile = dto.DocumentFile,
                IsActive = dto.IsActive,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Tenants.Add(entity);
            await _context.SaveChangesAsync();

            // Load the entity with navigation properties for response
            var createdEntity = await _context.Tenants
                .Include(t => t.Building)
                .Include(t => t.Floor)
                .Include(t => t.Unit)
                .Include(t => t.CreatedByUser)
                .FirstAsync(t => t.TenantId == entity.TenantId);

            return new TenantResponseDTO
            {
                TenantId = createdEntity.TenantId,
                TenantName = createdEntity.TenantName,
                BuildingId = createdEntity.BuildingId,
                BuildingName = createdEntity.Building.Name,
                Type = createdEntity.Type,
                FloorId = createdEntity.FloorId,
                FloorName = createdEntity.Floor?.Name,
                UnitId = createdEntity.UnitId,
                UnitNumber = createdEntity.Unit?.UnitNumber,
                ContractStartDate = createdEntity.ContractStartDate,
                ContractEndDate = createdEntity.ContractEndDate,
                IsPaid = createdEntity.IsPaid,
                MemberType = createdEntity.MemberType,
                DocumentFile = createdEntity.DocumentFile,
                IsActive = createdEntity.IsActive,
                CreatedAt = createdEntity.CreatedAt,
                UpdatedAt = createdEntity.UpdatedAt,
                CreatedByName = createdEntity.CreatedByUser.Name
            };
        }

        public async Task<TenantResponseDTO?> UpdateTenantAsync(int tenantId, UpdateTenantDTO dto, int currentUserId)
        {
            var query = _context.Tenants.Where(t => t.TenantId == tenantId);
            query = await ApplyAccessControlAsync(query, currentUserId);

            var entity = await query.FirstOrDefaultAsync();
            if (entity == null) return null;

            // Determine the target building ID for validation
            var targetBuildingId = dto.BuildingId ?? entity.BuildingId;

            // Validate building exists if being changed
            if (dto.BuildingId.HasValue && dto.BuildingId.Value != entity.BuildingId)
            {
                var buildingExists = await _context.Buildings.AnyAsync(b => b.BuildingId == dto.BuildingId.Value);
                if (!buildingExists)
                    throw new ArgumentException("Invalid BuildingId");
            }

            // Validate FloorId belongs to the target building if provided
            if (dto.FloorId.HasValue)
            {
                var floorBelongsToBuilding = await _context.Floors
                    .AnyAsync(f => f.FloorId == dto.FloorId.Value && f.BuildingId == targetBuildingId);
                if (!floorBelongsToBuilding)
                    throw new ArgumentException("FloorId does not belong to the specified BuildingId");
            }

            // Validate UnitId belongs to the target building (and floor if provided) if provided
            if (dto.UnitId.HasValue)
            {
                var targetFloorId = dto.FloorId ?? entity.FloorId;
                var unitQuery = _context.Units.Where(u => u.UnitId == dto.UnitId.Value && u.BuildingId == targetBuildingId);
                if (targetFloorId.HasValue)
                {
                    unitQuery = unitQuery.Where(u => u.FloorId == targetFloorId.Value);
                }
                var unitBelongsToBuilding = await unitQuery.AnyAsync();
                if (!unitBelongsToBuilding)
                    throw new ArgumentException("UnitId does not belong to the specified BuildingId/FloorId");
            }

            if (dto.TenantName != null) entity.TenantName = dto.TenantName;
            if (dto.BuildingId.HasValue) entity.BuildingId = dto.BuildingId.Value;
            if (dto.Type != null) entity.Type = dto.Type;
            if (dto.FloorId.HasValue) entity.FloorId = dto.FloorId.Value;
            if (dto.UnitId.HasValue) entity.UnitId = dto.UnitId.Value;
            if (dto.ContractStartDate.HasValue) entity.ContractStartDate = dto.ContractStartDate.Value;
            if (dto.ContractEndDate.HasValue) entity.ContractEndDate = dto.ContractEndDate.Value;
            if (dto.IsPaid.HasValue) entity.IsPaid = dto.IsPaid.Value;
            if (dto.MemberType != null) entity.MemberType = dto.MemberType;
            if (dto.DocumentFile != null) entity.DocumentFile = dto.DocumentFile;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Load the entity with navigation properties for response
            var updatedEntity = await _context.Tenants
                .Include(t => t.Building)
                .Include(t => t.Floor)
                .Include(t => t.Unit)
                .Include(t => t.CreatedByUser)
                .FirstAsync(t => t.TenantId == entity.TenantId);

            return new TenantResponseDTO
            {
                TenantId = updatedEntity.TenantId,
                TenantName = updatedEntity.TenantName,
                BuildingId = updatedEntity.BuildingId,
                BuildingName = updatedEntity.Building.Name,
                Type = updatedEntity.Type,
                FloorId = updatedEntity.FloorId,
                FloorName = updatedEntity.Floor?.Name,
                UnitId = updatedEntity.UnitId,
                UnitNumber = updatedEntity.Unit?.UnitNumber,
                ContractStartDate = updatedEntity.ContractStartDate,
                ContractEndDate = updatedEntity.ContractEndDate,
                IsPaid = updatedEntity.IsPaid,
                MemberType = updatedEntity.MemberType,
                DocumentFile = updatedEntity.DocumentFile,
                IsActive = updatedEntity.IsActive,
                CreatedAt = updatedEntity.CreatedAt,
                UpdatedAt = updatedEntity.UpdatedAt,
                CreatedByName = updatedEntity.CreatedByUser.Name
            };
        }

        public async Task<bool> ActivateTenantAsync(int tenantId, int currentUserId)
        {
            var query = _context.Tenants.Where(t => t.TenantId == tenantId);
            query = await ApplyAccessControlAsync(query, currentUserId);
            var entity = await query.FirstOrDefaultAsync();
            if (entity == null) return false;
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateTenantAsync(int tenantId, int currentUserId)
        {
            var query = _context.Tenants.Where(t => t.TenantId == tenantId);
            query = await ApplyAccessControlAsync(query, currentUserId);
            var entity = await query.FirstOrDefaultAsync();
            if (entity == null) return false;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> InviteTenantAsync(InviteTenantDTO dto, int currentUserId)
        {
            // Determine user type for the tenant invitation
            int userTypeId;
            if (dto.UserTypeId.HasValue)
            {
                userTypeId = dto.UserTypeId.Value;
            }
            else
            {
                // Try to find a PM-created user type named "Tenant"
                var candidate = await _context.UserTypes
                    .Where(ut => ut.Name == "Tenant" && ut.IsActive && ut.DataAccessControl == UserTypeConstants.DATA_ACCESS_PM)
                    .Select(ut => (int?)ut.Id)
                    .FirstOrDefaultAsync();
                if (candidate == null)
                {
                    throw new ArgumentException("No suitable 'Tenant' user type found. Please create a user type named 'Tenant' with DataAccessControl = 'PM' or provide UserTypeId.");
                }
                userTypeId = candidate.Value;
            }

            // Use the existing Invitation service to create invitation with selected building
            var createInvitation = new CreateInvitationDTO
            {
                EmailAddress = dto.Email,
                UserTypeId = userTypeId,
                ExpiryDays = dto.ExpiryDays,
                BuildingIds = new List<System.Text.Json.JsonElement> // Use explicit building mapping
                {
                    System.Text.Json.JsonDocument.Parse(dto.BuildingId.ToString()).RootElement
                }
            };

            // Validate inviter permissions via invitation service rules
            var canInvite = await _invitationService.CanUserInviteUserTypeAsync(currentUserId, userTypeId);
            if (!canInvite) throw new UnauthorizedAccessException("You are not allowed to invite this user type.");

            // Note: InvitationService enforces PM access control and persists InvitationBuildings mapping
            var invitation = await _invitationService.CreateInvitationAsync(createInvitation, currentUserId);
            return invitation != null;
        }

    }
}
