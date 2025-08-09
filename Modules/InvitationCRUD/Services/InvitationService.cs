using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.InvitationCRUD.DTOs;
using YopoBackend.Modules.InvitationCRUD.Models;

namespace YopoBackend.Modules.InvitationCRUD.Services
{
    /// <summary>
    /// Implementation of invitation service
    /// </summary>
    public class InvitationService : IInvitationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvitationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public InvitationService(ApplicationDbContext context, ILogger<InvitationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InvitationResponseDTO>> GetAllInvitationsAsync()
        {
            var invitations = await _context.Invitations
                .Include(i => i.UserType)
                .ToListAsync();
            return invitations.Select(MapToResponseDTO);
        }

        /// <inheritdoc/>
        public async Task<InvitationResponseDTO?> GetInvitationByIdAsync(int id)
        {
            var invitation = await _context.Invitations
                .Include(i => i.UserType)
                .FirstOrDefaultAsync(i => i.Id == id);
            return invitation == null ? null : MapToResponseDTO(invitation);
        }

        /// <inheritdoc/>
        public async Task<InvitationResponseDTO> CreateInvitationAsync(CreateInvitationDTO createDto)
        {
            var invitation = new Invitation
            {
                EmailAddress = createDto.EmailAddress.ToLowerInvariant(),
                UserTypeId = createDto.UserTypeId,
                ExpiryTime = DateTime.UtcNow.AddDays(createDto.ExpiryDays),
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
        public async Task<InvitationResponseDTO?> UpdateInvitationAsync(int id, UpdateInvitationDTO updateDto)
        {
            var invitation = await _context.Invitations
                .Include(i => i.UserType)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invitation == null)
            {
                return null;
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
        public async Task<bool> DeleteInvitationAsync(int id)
        {
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return false;
            }

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted invitation ID: {Id}", id);
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InvitationResponseDTO>> GetExpiredInvitationsAsync()
        {
            var expiredInvitations = await _context.Invitations
                .Include(i => i.UserType)
                .Where(i => i.ExpiryTime < DateTime.UtcNow)
                .ToListAsync();

            return expiredInvitations.Select(MapToResponseDTO);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InvitationResponseDTO>> GetActiveInvitationsAsync()
        {
            var activeInvitations = await _context.Invitations
                .Include(i => i.UserType)
                .Where(i => i.ExpiryTime >= DateTime.UtcNow)
                .ToListAsync();

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
