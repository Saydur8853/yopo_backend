using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.InvitationCRUD.DTOs;
using YopoBackend.Modules.InvitationCRUD.Models;
using YopoBackend.Services;

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
        public async Task<IEnumerable<InvitationResponseDTO>> GetAllInvitationsAsync(int currentUserId)
        {
            var query = _context.Invitations.Include(i => i.UserType).AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var invitations = await query.ToListAsync();
            return invitations.Select(MapToResponseDTO);
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
            
            return MapToResponseDTO(invitation);
        }

        /// <inheritdoc/>
        public async Task<InvitationResponseDTO> CreateInvitationAsync(CreateInvitationDTO createDto, int createdByUserId)
        {
            var invitation = new Invitation
            {
                EmailAddress = createDto.EmailAddress.ToLowerInvariant(),
                UserTypeId = createDto.UserTypeId,
                ExpiryTime = DateTime.UtcNow.AddDays(createDto.ExpiryDays),
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Load the user type for the response
            await _context.Entry(invitation)
                .Reference(i => i.UserType)
                .LoadAsync();

            _logger.LogInformation("Created invitation for email: {Email} with UserType: {UserTypeId}", 
                createDto.EmailAddress, createDto.UserTypeId);
            return MapToResponseDTO(invitation);
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

            if (updateDto.ExpiryDays.HasValue)
            {
                invitation.ExpiryTime = DateTime.UtcNow.AddDays(updateDto.ExpiryDays.Value);
            }

            invitation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload the user type if it was changed
            if (updateDto.UserTypeId.HasValue)
            {
                await _context.Entry(invitation)
                    .Reference(i => i.UserType)
                    .LoadAsync();
            }

            _logger.LogInformation("Updated invitation ID: {Id}", id);
            return MapToResponseDTO(invitation);
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
            return expiredInvitations.Select(MapToResponseDTO);
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
            return activeInvitations.Select(MapToResponseDTO);
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
        public async Task<IEnumerable<UserTypeDropdownDTO>> GetAvailableUserTypesAsync()
        {
            var userTypes = await _context.UserTypes
                .Where(ut => ut.IsActive)
                .Select(ut => new UserTypeDropdownDTO
                {
                    Id = ut.Id,
                    Name = ut.Name,
                    IsActive = ut.IsActive
                })
                .OrderBy(ut => ut.Name)
                .ToListAsync();

            return userTypes;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateUserTypeIdAsync(int userTypeId)
        {
            return await _context.UserTypes
                .AnyAsync(ut => ut.Id == userTypeId && ut.IsActive);
        }

        private static InvitationResponseDTO MapToResponseDTO(Invitation invitation)
        {
            return new InvitationResponseDTO
            {
                Id = invitation.Id,
                EmailAddress = invitation.EmailAddress,
                UserTypeId = invitation.UserTypeId,
                UserTypeName = invitation.UserType?.Name ?? "Unknown",
                ExpiryTime = invitation.ExpiryTime,
                CreatedAt = invitation.CreatedAt,
                UpdatedAt = invitation.UpdatedAt,
                IsExpired = invitation.IsExpired,
                DaysUntilExpiry = invitation.DaysUntilExpiry
            };
        }
    }
}
