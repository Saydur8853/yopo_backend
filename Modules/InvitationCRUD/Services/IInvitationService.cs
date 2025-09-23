using YopoBackend.Modules.InvitationCRUD.DTOs;
using YopoBackend.Modules.InvitationCRUD.Models;

namespace YopoBackend.Modules.InvitationCRUD.Services
{
    /// <summary>
    /// Interface for invitation service operations
    /// </summary>
    public interface IInvitationService
    {
        /// <summary>
        /// Gets all invitations asynchronously with access control and pagination.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <param name="page">The page number (starting from 1).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="searchTerm">Optional search term to filter invitations by email address.</param>
        /// <param name="userTypeId">Optional user type ID to filter invitations.</param>
        /// <param name="isExpired">Optional filter by expiration status (true for expired, false for active, null for all).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated invitation list response DTO.</returns>
        Task<InvitationListResponseDTO> GetAllInvitationsAsync(int currentUserId, int page = 1, int pageSize = 10, string? searchTerm = null, int? userTypeId = null, bool? isExpired = null);
        
        /// <summary>
        /// Gets all invitations asynchronously with access control (non-paginated - for backwards compatibility).
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetAllInvitationsListAsync(int currentUserId);
        
        /// <summary>
        /// Gets an invitation by its ID asynchronously with access control.
        /// </summary>
        /// <param name="id">The invitation ID.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the invitation response DTO, or null if not found.</returns>
        Task<InvitationResponseDTO?> GetInvitationByIdAsync(int id, int currentUserId);
        
        /// <summary>
        /// Creates a new invitation asynchronously.
        /// </summary>
        /// <param name="createDto">The invitation creation data.</param>
        /// <param name="createdByUserId">The ID of the user creating this invitation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created invitation response DTO.</returns>
        Task<InvitationResponseDTO> CreateInvitationAsync(CreateInvitationDTO createDto, int createdByUserId);
        
        /// <summary>
        /// Updates an existing invitation asynchronously with access control.
        /// </summary>
        /// <param name="id">The invitation ID to update.</param>
        /// <param name="updateDto">The invitation update data.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated invitation response DTO, or null if not found.</returns>
        Task<InvitationResponseDTO?> UpdateInvitationAsync(int id, UpdateInvitationDTO updateDto, int currentUserId);
        
        /// <summary>
        /// Deletes an invitation asynchronously with access control.
        /// </summary>
        /// <param name="id">The invitation ID to delete.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
        Task<bool> DeleteInvitationAsync(int id, int currentUserId);
        
        /// <summary>
        /// Gets all expired invitations asynchronously with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of expired invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetExpiredInvitationsAsync(int currentUserId);
        
        /// <summary>
        /// Gets all active (non-expired) invitations asynchronously with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetActiveInvitationsAsync(int currentUserId);
        
        /// <summary>
        /// Checks if an invitation exists asynchronously.
        /// </summary>
        /// <param name="id">The invitation ID to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the invitation exists.</returns>
        Task<bool> InvitationExistsAsync(int id);
        
        /// <summary>
        /// Checks if an email address already has an active invitation asynchronously.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the email already has an active invitation.</returns>
        Task<bool> EmailAlreadyInvitedAsync(string email);
        
        /// <summary>
        /// Checks if an email address already belongs to an existing registered user.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if a user with this email exists; otherwise, false.</returns>
        Task<bool> EmailAlreadyRegisteredAsync(string email);
        
        /// <summary>
        /// Checks if there is an active Property Manager invitation with the given company name.
        /// </summary>
        /// <param name="companyName">The company name to check.</param>
        /// <returns>True if a PM invitation already exists for this company; otherwise, false.</returns>
        Task<bool> CompanyAlreadyInvitedAsync(string companyName);
        
        /// <summary>
        /// Checks if there is a registered Property Manager (Customer) with the given company name.
        /// </summary>
        /// <param name="companyName">The company name to check.</param>
        /// <returns>True if a PM is already registered for this company; otherwise, false.</returns>
        Task<bool> CompanyAlreadyRegisteredAsync(string companyName);
        
        /// <summary>
        /// Gets all available user types for invitation dropdown selection with access control.
        /// 
        /// **Security Note:** This method only returns user types that are allowed for invitations:
        /// - Super Admin (full system access)
        /// - Property Manager (limited access with own data control)
        /// 
        /// The restriction is enforced at the service level for security purposes.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of available user types (filtered to allowed types only).</returns>
        Task<IEnumerable<UserTypeDropdownDTO>> GetAvailableUserTypesAsync(int currentUserId);
        
        /// <summary>
        /// Validates if a user type ID exists and is active.
        /// </summary>
        /// <param name="userTypeId">The user type ID to validate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the user type ID is valid.</returns>
        Task<bool> ValidateUserTypeIdAsync(int userTypeId);
        
        /// <summary>
        /// Checks if the current user has permission to invite users of the specified type.
        /// Business Rules:
        /// - Super Admins can invite anyone (Super Admins, Property Managers, PM-created types)
        /// - Property Managers can only invite PM-created user types (NOT other Property Managers or Super Admins)
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <param name="targetUserTypeId">The user type ID they want to invite.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the user can invite the specified user type.</returns>
        Task<bool> CanUserInviteUserTypeAsync(int currentUserId, int targetUserTypeId);
    }
}
