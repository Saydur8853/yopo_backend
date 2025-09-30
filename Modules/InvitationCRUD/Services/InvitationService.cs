using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.InvitationCRUD.DTOs;
using YopoBackend.Modules.InvitationCRUD.Models;
using YopoBackend.Services;
using YopoBackend.Constants;

namespace YopoBackend.Modules.InvitationCRUD.Services
{
    /// <summary>
    /// Implementation of invitation service with Data Access Control
    /// </summary>
    public class InvitationService : BaseAccessControlService, IInvitationService
    {
        private readonly ILogger<InvitationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public InvitationService(ApplicationDbContext context, ILogger<InvitationService> logger) : base(context)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<InvitationListResponseDTO> GetAllInvitationsAsync(int currentUserId, int page = 1, int pageSize = 10, string? searchTerm = null, int? userTypeId = null, bool? isExpired = null)
        {
            var query = _context.Invitations.Include(i => i.UserType).AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLowerInvariant();
                query = query.Where(i => i.EmailAddress.ToLower().Contains(lowerSearchTerm));
            }
            
            if (userTypeId.HasValue)
            {
                query = query.Where(i => i.UserTypeId == userTypeId.Value);
            }
            
            if (isExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (isExpired.Value)
                {
                    query = query.Where(i => i.ExpiryTime < now);
                }
                else
                {
                    query = query.Where(i => i.ExpiryTime >= now);
                }
            }
            
            // Get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var invitations = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
            
            // Prefetch building IDs for all invitations
            var invitationIds = invitations.Select(i => i.Id).ToList();
            var buildingLookup = await _context.InvitationBuildings
                .Where(ib => invitationIds.Contains(ib.InvitationId))
                .GroupBy(ib => ib.InvitationId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.BuildingId).ToList());

            var invitationDtos = invitations
                .Select(inv => FilterCompanyNameForUser(
                    MapToResponseDTO(inv, buildingLookup.GetValueOrDefault(inv.Id)),
                    isSuperAdmin))
                .ToList();
            
            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return new InvitationListResponseDTO
            {
                Invitations = invitationDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<InvitationResponseDTO>> GetAllInvitationsListAsync(int currentUserId)
        {
            var query = _context.Invitations.Include(i => i.UserType).AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var invitations = await query.ToListAsync();
            
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
            
            // Prefetch building IDs for all invitations
            var invitationIds = invitations.Select(i => i.Id).ToList();
            var buildingLookup = await _context.InvitationBuildings
                .Where(ib => invitationIds.Contains(ib.InvitationId))
                .GroupBy(ib => ib.InvitationId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.BuildingId).ToList());

            return invitations.Select(inv => FilterCompanyNameForUser(
                MapToResponseDTO(inv, buildingLookup.GetValueOrDefault(inv.Id)),
                isSuperAdmin));
        }

        /// <inheritdoc/>
        public async Task<InvitationResponseDTO?> GetInvitationByIdAsync(int id, int currentUserId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.UserType)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (invitation == null)
            {
                return null;
            }
            
            // Check access control
            if (!await HasAccessToEntityAsync(invitation, currentUserId))
            {
                return null; // User doesn't have access to this invitation
            }
            
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
            
            // Load building IDs for this invitation
            var buildingIds = await _context.InvitationBuildings
                .Where(ib => ib.InvitationId == invitation.Id)
                .Select(ib => ib.BuildingId)
                .ToListAsync();

            return FilterCompanyNameForUser(MapToResponseDTO(invitation, buildingIds), isSuperAdmin);
        }

        /// <inheritdoc/>
        public async Task<InvitationResponseDTO> CreateInvitationAsync(CreateInvitationDTO createDto, int createdByUserId)
        {
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(createdByUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
            
            var invitation = new Invitation
            {
                EmailAddress = createDto.EmailAddress.ToLowerInvariant(),
                UserTypeId = createDto.UserTypeId,
                // Only Super Admins can set CompanyName
                CompanyName = isSuperAdmin ? createDto.CompanyName : null,
                ExpiryTime = DateTime.UtcNow.AddDays(createDto.ExpiryDays),
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // If inviter is a PM and buildings were specified (for non-PM invite), persist mapping
            if (!isSuperAdmin && createDto.UserTypeId != UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && createDto.BuildingIds != null && createDto.BuildingIds.Any())
            {
                // Determine buildings to assign: handle ["all"] or explicit list
                var pmId = currentUser!.Id;
                var resolvedIds = await ResolveBuildingIdsAsync(createDto.BuildingIds, pmId);

                if (resolvedIds.Any())
                {
                    var toInsert = resolvedIds.Select(bid => new YopoBackend.Modules.InvitationCRUD.Models.InvitationBuilding
                    {
                        InvitationId = invitation.Id,
                        BuildingId = bid,
                        CreatedAt = DateTime.UtcNow
                    });
                    _context.InvitationBuildings.AddRange(toInsert);
                    await _context.SaveChangesAsync();
                }
            }

            // Load the user type for the response
            await _context.Entry(invitation)
                .Reference(i => i.UserType)
                .LoadAsync();

            _logger.LogInformation("Created invitation for email: {Email} with UserType: {UserTypeId}", 
                createDto.EmailAddress, createDto.UserTypeId);
            
            // Return with proper CompanyName filtering (Super Admin check already done above)
            var responseUser = await GetUserWithAccessControlAsync(createdByUserId);
            bool isResponseSuperAdmin = responseUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;

            // Load building IDs for this invitation
            var buildingIdsForNew = await _context.InvitationBuildings
                .Where(ib => ib.InvitationId == invitation.Id)
                .Select(ib => ib.BuildingId)
                .ToListAsync();
            
            return FilterCompanyNameForUser(MapToResponseDTO(invitation, buildingIdsForNew), isResponseSuperAdmin);
        }

        /// <inheritdoc/>
        public async Task<InvitationResponseDTO?> UpdateInvitationAsync(int id, UpdateInvitationDTO updateDto, int currentUserId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.UserType)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invitation == null)
            {
                return null;
            }
            
            // Check access control
            if (!await HasAccessToEntityAsync(invitation, currentUserId))
            {
                return null; // User doesn't have access to update this invitation
            }

            if (!string.IsNullOrEmpty(updateDto.EmailAddress))
            {
                invitation.EmailAddress = updateDto.EmailAddress.ToLowerInvariant();
            }

            if (updateDto.UserTypeId.HasValue)
            {
                invitation.UserTypeId = updateDto.UserTypeId.Value;
            }

            if (updateDto.CompanyName != null)
            {
                // Check if user is Super Admin for CompanyName access
                var updateUser = await GetUserWithAccessControlAsync(currentUserId);
                bool isUpdateSuperAdmin = updateUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
                
                // Only Super Admins can update CompanyName
                if (isUpdateSuperAdmin)
                {
                    invitation.CompanyName = updateDto.CompanyName;
                }
            }

            if (updateDto.ExpiryDays.HasValue)
            {
                invitation.ExpiryTime = DateTime.UtcNow.AddDays(updateDto.ExpiryDays.Value);
            }

            invitation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update building mappings if requested
            if (updateDto.BuildingIds != null)
            {
                // Determine PM context for validation
                int? pmId = await _context.Users
                    .Where(u => u.Id == invitation.CreatedBy && u.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                    .Select(u => (int?)u.Id)
                    .FirstOrDefaultAsync();

                if (pmId == null)
                {
                    var acting = await GetUserWithAccessControlAsync(currentUserId);
                    if (acting?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                    {
                        pmId = acting.Id;
                    }
                }

                // Clear existing mappings
                var existing = await _context.InvitationBuildings
                    .Where(ib => ib.InvitationId == id)
                    .ToListAsync();
                _context.InvitationBuildings.RemoveRange(existing);

                // Only apply if target user type is not Property Manager and pmId is available
                if (invitation.UserTypeId != UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && pmId.HasValue && updateDto.BuildingIds.Any())
                {
                    var resolvedIds = await ResolveBuildingIdsAsync(updateDto.BuildingIds, pmId.Value);

                    var toInsert = resolvedIds.Select(bid => new YopoBackend.Modules.InvitationCRUD.Models.InvitationBuilding
                    {
                        InvitationId = invitation.Id,
                        BuildingId = bid,
                        CreatedAt = DateTime.UtcNow
                    });
                    _context.InvitationBuildings.AddRange(toInsert);
                }

                await _context.SaveChangesAsync();
            }

            // Reload the user type if it was changed
            if (updateDto.UserTypeId.HasValue)
            {
                await _context.Entry(invitation)
                    .Reference(i => i.UserType)
                    .LoadAsync();
            }

            _logger.LogInformation("Updated invitation ID: {Id}", id);
            
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
            
            // Load building IDs for this invitation
            var buildingIds = await _context.InvitationBuildings
                .Where(ib => ib.InvitationId == invitation.Id)
                .Select(ib => ib.BuildingId)
                .ToListAsync();

            return FilterCompanyNameForUser(MapToResponseDTO(invitation, buildingIds), isSuperAdmin);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteInvitationAsync(int id, int currentUserId)
        {
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return false;
            }
            
            // Check access control
            if (!await HasAccessToEntityAsync(invitation, currentUserId))
            {
                return false; // User doesn't have access to delete this invitation
            }

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted invitation ID: {Id}", id);
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InvitationResponseDTO>> GetExpiredInvitationsAsync(int currentUserId)
        {
            var query = _context.Invitations
                .Include(i => i.UserType)
                .Where(i => i.ExpiryTime < DateTime.UtcNow)
                .AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var expiredInvitations = await query.ToListAsync();
            
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;

            // Prefetch building IDs
            var expIds = expiredInvitations.Select(i => i.Id).ToList();
            var expBldLookup = await _context.InvitationBuildings
                .Where(ib => expIds.Contains(ib.InvitationId))
                .GroupBy(ib => ib.InvitationId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.BuildingId).ToList());
            
            return expiredInvitations.Select(inv => FilterCompanyNameForUser(
                MapToResponseDTO(inv, expBldLookup.GetValueOrDefault(inv.Id)), isSuperAdmin));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InvitationResponseDTO>> GetActiveInvitationsAsync(int currentUserId)
        {
            var query = _context.Invitations
                .Include(i => i.UserType)
                .Where(i => i.ExpiryTime >= DateTime.UtcNow)
                .AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var activeInvitations = await query.ToListAsync();
            
            // Check if user is Super Admin for CompanyName access
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isSuperAdmin = currentUser?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;

            // Prefetch building IDs
            var actIds = activeInvitations.Select(i => i.Id).ToList();
            var actBldLookup = await _context.InvitationBuildings
                .Where(ib => actIds.Contains(ib.InvitationId))
                .GroupBy(ib => ib.InvitationId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.BuildingId).ToList());
            
            return activeInvitations.Select(inv => FilterCompanyNameForUser(
                MapToResponseDTO(inv, actBldLookup.GetValueOrDefault(inv.Id)), isSuperAdmin));
        }

        /// <inheritdoc/>
        public async Task<bool> InvitationExistsAsync(int id)
        {
            return await _context.Invitations.AnyAsync(i => i.Id == id);
        }

        /// <inheritdoc/>
        public async Task<bool> EmailAlreadyInvitedAsync(string email)
        {
            return await _context.Invitations
                .AnyAsync(i => i.EmailAddress == email.ToLowerInvariant() && i.ExpiryTime >= DateTime.UtcNow);
        }

        /// <inheritdoc/>
        public async Task<bool> EmailAlreadyRegisteredAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant());
        }

        /// <inheritdoc/>
        public async Task<bool> CompanyAlreadyInvitedAsync(string companyName)
        {
            var lower = companyName.ToLowerInvariant();
            return await _context.Invitations.AnyAsync(i => 
                i.CompanyName != null && i.CompanyName.ToLower() == lower &&
                i.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID &&
                i.ExpiryTime >= DateTime.UtcNow);
        }

        /// <inheritdoc/>
        public async Task<bool> CompanyAlreadyRegisteredAsync(string companyName)
        {
            var lower = companyName.ToLowerInvariant();
            return await _context.Customers.AnyAsync(c => c.CompanyName != null && c.CompanyName.ToLower() == lower);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeDropdownDTO>> GetAvailableUserTypesAsync(int currentUserId)
        {
            var query = _context.UserTypes
                .Where(ut => ut.IsActive)
                .AsQueryable();
            
            // Apply access control - users with "OWN" access control can only see user types they created
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var userTypes = await query
                .Select(ut => new UserTypeDropdownDTO
                {
                    Id = ut.Id,
                    Name = ut.Name,
                    IsActive = ut.IsActive
                })
                .OrderBy(ut => ut.Name)
                .ToListAsync();

            // API-level restriction: Allow specific user types for invitations
            // This ensures security even if frontend filtering is removed
            // 
            // BUSINESS LOGIC: For security and system integrity, invitations are restricted to:
            // - Super Admin: Full system access, can manage everything
            // - Property Manager: Limited access with OWN data access control
            // - PM-created user types: User types created by Property Managers (DataAccessControl = "PM")
            // 
            // This allows Property Managers to invite users to their custom user types
            // while maintaining the intended user hierarchy and security in the system.
            
            // Get the full user type details to check DataAccessControl
            var userTypeIds = userTypes.Select(ut => ut.Id).ToList();
            var fullUserTypes = await _context.UserTypes
                .Where(ut => userTypeIds.Contains(ut.Id))
                .Select(ut => new { ut.Id, ut.Name, ut.DataAccessControl })
                .ToListAsync();
            
            // First filter by allowed user types for invitations
            var allowedUserTypes = userTypes.Where(ut => 
            {
                var fullUserType = fullUserTypes.FirstOrDefault(fut => fut.Id == ut.Id);
                return fullUserType != null && (
                    // Allow default system user types
                    ut.Name == "Super Admin" || ut.Name == "Property Manager" ||
                    // Allow PM-created user types (those with DataAccessControl = "PM")
                    fullUserType.DataAccessControl == "PM"
                );
            }).ToList();
            
            // Further filter based on current user's permissions
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser != null)
            {
                // If user is Property Manager, exclude Super Admin and Property Manager from the list
                if (currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                {
                    allowedUserTypes = allowedUserTypes.Where(ut => 
                        ut.Name != "Super Admin" && ut.Name != "Property Manager"
                    ).ToList();
                }
                // For PM-created user types, also exclude Super Admin and Property Manager
                else if (currentUser.UserType?.DataAccessControl == "PM")
                {
                    allowedUserTypes = allowedUserTypes.Where(ut => 
                        ut.Name != "Super Admin" && ut.Name != "Property Manager"
                    ).ToList();
                }
                // Super Admins can only see default system user types (Super Admin + Property Manager)
                if (currentUser.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
                {
                    var defaultIds = new[] { UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID, UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID };
                    allowedUserTypes = allowedUserTypes.Where(ut => defaultIds.Contains(ut.Id)).ToList();
                }
            }

            return allowedUserTypes;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateUserTypeIdAsync(int userTypeId)
        {
            var userType = await _context.UserTypes
                .FirstOrDefaultAsync(ut => ut.Id == userTypeId && ut.IsActive);
                
            if (userType == null)
                return false;
                
            // Allow validation for:
            // - Super Admin (ID: 1)
            // - Property Manager (ID: 2) 
            // - PM-created user types (DataAccessControl = "PM")
            return userType.Name == "Super Admin" || 
                   userType.Name == "Property Manager" || 
                   userType.DataAccessControl == "PM";
        }
        
        /// <inheritdoc/>
        public async Task<bool> CanUserInviteUserTypeAsync(int currentUserId, int targetUserTypeId)
        {
            // Get current user information
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null) return false;
            
            // Get target user type information
            var targetUserType = await _context.UserTypes
                .FirstOrDefaultAsync(ut => ut.Id == targetUserTypeId && ut.IsActive);
            if (targetUserType == null) return false;
            
            // Super Admins can only invite default system user types
            if (currentUser.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return targetUserType.Id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID ||
                       targetUserType.Id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID;
            }
            
            // Property Managers have restrictions
            if (currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                // Property Managers CANNOT invite:
                // - Other Property Managers
                // - Super Admins
                if (targetUserType.Name == "Property Manager" || targetUserType.Name == "Super Admin")
                {
                    return false;
                }
                
                // Property Managers CAN only invite PM-created user types
                return targetUserType.DataAccessControl == "PM";
            }
            
            // Users with PM-created user types can only invite within their PM ecosystem
            // but cannot invite Property Managers or Super Admins
            if (currentUser.UserType?.DataAccessControl == "PM")
            {
                if (targetUserType.Name == "Property Manager" || targetUserType.Name == "Super Admin")
                {
                    return false;
                }
                return targetUserType.DataAccessControl == "PM";
            }
            
            // Default: deny access for any other user types
            return false;
        }

        private InvitationResponseDTO MapToResponseDTO(Invitation invitation, List<int>? buildingIds, int? requestingUserId = null)
        {
            return new InvitationResponseDTO
            {
                Id = invitation.Id,
                EmailAddress = invitation.EmailAddress,
                UserTypeId = invitation.UserTypeId,
                UserTypeName = invitation.UserType?.Name ?? "Unknown",
                CompanyName = invitation.CompanyName, // Will be filtered in service methods
                ExpiryTime = invitation.ExpiryTime,
                CreatedAt = invitation.CreatedAt,
                UpdatedAt = invitation.UpdatedAt,
                IsExpired = invitation.IsExpired,
                DaysUntilExpiry = invitation.DaysUntilExpiry,
                BuildingIds = buildingIds ?? new List<int>()
            };
        }
        
        /// <summary>
        /// Filters CompanyName field based on user permissions - only Super Admins can see CompanyName.
        /// </summary>
        /// <param name="invitation">The invitation response DTO</param>
        /// <param name="isSuperAdmin">Whether the requesting user is a Super Admin</param>
        /// <returns>The invitation DTO with CompanyName filtered if not Super Admin</returns>
        private static InvitationResponseDTO FilterCompanyNameForUser(InvitationResponseDTO invitation, bool isSuperAdmin)
        {
            if (!isSuperAdmin)
            {
                invitation.CompanyName = null; // Hide company name from non-Super Admin users
            }
            return invitation;
        }
        private async Task<List<int>> ResolveBuildingIdsAsync(IEnumerable<System.Text.Json.JsonElement> rawBuildingIds, int pmId)
        {
            var list = rawBuildingIds.ToList();
            if (list.Count == 1 && list[0].ValueKind == System.Text.Json.JsonValueKind.String &&
                string.Equals(list[0].GetString(), "all", StringComparison.OrdinalIgnoreCase))
            {
                return await _context.Buildings
                    .Where(b => b.CustomerId == pmId)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
            }

            // Parse numeric ids safely from numbers or numeric strings
            var numeric = new List<int>();
            foreach (var el in list)
            {
                if (el.ValueKind == System.Text.Json.JsonValueKind.Number && el.TryGetInt32(out var n))
                {
                    numeric.Add(n);
                }
                else if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var s = el.GetString();
                    if (int.TryParse(s, out var m)) numeric.Add(m);
                }
            }

            if (!numeric.Any()) return new List<int>();

            // Validate against PM's buildings
            var valid = await _context.Buildings
                .Where(b => b.CustomerId == pmId && numeric.Contains(b.BuildingId))
                .Select(b => b.BuildingId)
                .ToListAsync();
            return valid;
        }
    }
}
